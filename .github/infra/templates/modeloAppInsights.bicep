@description('Nome do aplication')
param applicationInsightsName string 

@description('Nome do workspace de armazenamento')
param workspaceName string 

param location string = resourceGroup().location
param product string
param project string
param ambiente string
param cost_center string

resource workspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: workspaceName
  location: location
  tags: {
    product: product
    project: project
    environment: ambiente
    cost_center: cost_center
    built_by: 'IaC'
  }
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
  tags: {
    product: product
    project: project
    environment: ambiente
    cost_center: cost_center
    built_by: 'IaC'
  }
}

output applicationInsightName string = applicationInsights.name
output applicationConection string = applicationInsights.properties.InstrumentationKey
