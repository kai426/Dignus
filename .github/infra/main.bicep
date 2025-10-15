param appName string 
param location string = resourceGroup().location
param sku string 
param product string 
param project string 
param ambiente string
param dockerImage string
param dockerRepos string
param tipo string
param cost_center string


module acr 'templates/modeloACR.bicep' = {
  name: 'acr'
  params:{
    location: location
    acrName :  toLower('acr${appName}${ambiente}') 
    product: product
    project: project
    ambiente: ambiente
    cost_center: cost_center
  }
}

module planoservice 'templates/modelo_plano_service.bicep' = {
  name:'planoservice'
  params:{
    appName: '${appName}-Plan-${tipo}-${ambiente}'
    location:location
    sku: sku
    product: product
    project: project
    ambiente: ambiente
    cost_center: cost_center
  }
}

module aplicationDocker 'templates/modelo_aplicacao_docker.bicep' = {
  name: 'aplicationDocker'
  params:{
    webAppname:'${appName}-${tipo}-${ambiente}'
    location: location
    acrname: acr.outputs.acrName
    appServicePlan: planoservice.outputs.appServicePlanId
    dockerImage: dockerImage
    dockerRepos: dockerRepos
    product: product
    project: project
    ambiente: ambiente
    cost_center: cost_center
    applicationkey: appInsights.outputs.applicationConection
  }
}

module appInsights 'templates/modeloAppInsights.bicep' = {
  name:'appInsights'
  params:{
    applicationInsightsName: toLower('${appName}-${ambiente}') 
    workspaceName:appName
    location: location
    product: product
    project: project
    ambiente: ambiente
    cost_center: cost_center
  }
}

output webAppName string = aplicationDocker.outputs.webAppName
output acrName string = acr.outputs.acrName
