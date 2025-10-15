
@description('Tipo de camada do Plano de Serviço')
param sku string 

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name that will be used to build associated artifacts')
param appName string 
param product string
param project string
param ambiente string
param cost_center string

var appServicePlanName = appName

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: sku
  }
  properties: {
    reserved: true
  }
  tags: {
    product: product
    project: project
    environment: ambiente
    cost_center: cost_center
    built_by: 'IaC'
  }

  
}

output appServicePlanId string = appServicePlan.id

