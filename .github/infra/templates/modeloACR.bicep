@description('Location for all resources.')
param location string = resourceGroup().location

// azure container registry
@description('Nome do ACR a ser criado')
param acrName string
param product string
param project string
param ambiente string
param cost_center string

@description('Plano do ACR(Tier)')
param acrSku string = 'Standard'

resource acr 'Microsoft.ContainerRegistry/registries@2021-09-01' = {
  name: replace(acrName,'-','')
  location: location
  tags: {
    product: product
    project: project
    environment: ambiente
    cost_center: cost_center
    built_by: 'IaC'
  }
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: true
  }
}

output acrLoginServer string = acr.properties.loginServer
output acrName string = acr.name

