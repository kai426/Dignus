param webAppname string
param location string = resourceGroup().location
param acrname string
param appServicePlan string
param dockerImage string
param dockerRepos string
param imgname string = '${acrname}.azurecr.io/${dockerRepos}:${dockerImage}'
param product string
param project string
param ambiente string
param cost_center string
param applicationkey string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-09-01' existing = {
  name: acrname
}


var ipRestrictions = toLower(ambiente) == 'prd' || toLower(ambiente) == 'qas' ? [
  {
    name: 'AllowInternal'
    priority: 100
    action: 'Allow'
    ipAddress: '177.66.156.0/23' // Restrição rede Bemol
    tag: 'Default'
    description: 'Permitir acesso por rede interna'
  }
  {
    name: 'DenyAll'
    priority: 200
    action: 'Deny'
    ipAddress: '0.0.0.0/0'
    tag: 'Default'
    description: 'Negar acesso externo'
  }
] : []


resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppname
  location: location
  tags: {
    product: product
    project: project
    environment: ambiente
    cost_center: cost_center
    built_by: 'IaC'
  }
  properties: {
    siteConfig: {
      alwaysOn: true
      appSettings: [
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: containerRegistry.properties.loginServer
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: containerRegistry.listCredentials().username
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationkey
        }
      ]
      linuxFxVersion: 'DOCKER|${imgname}'
      ipSecurityRestrictions: ipRestrictions
    }
    serverFarmId: appServicePlan
  }
}

output webAppName string = webApp.name
