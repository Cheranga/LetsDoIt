targetScope = 'subscription'

param appName string
param version string
param location string


@allowed([
  'dev'
  'qa'
])
param environment string

var envType = {
    dev: 'nonprod'
    qa: 'nonprod'
    prod: 'prod'
    } 

var appNameWithEnvironment = '${appName}-${environment}'
var rgName = 'cchat-rg-${appNameWithEnvironment}'
var funcAppName = 'cchat-fn-${appNameWithEnvironment}'
var sgName = take(replace('cchatsg${appNameWithEnvironment}', '-', ''), 24)
var appInsName = 'cchat-ins-${appNameWithEnvironment}'
var aspName = 'cchat-asp-${appNameWithEnvironment}'
var kvName = take(replace('cchatkv${appNameWithEnvironment}', '-', ''), 24)

module rg 'resourcegroup/template.bicep' = {
  scope: subscription()
  name: '${version}-rg'
  params: {
    location: location
    name: rgName
  }
}

module storageAccount 'storageaccount/template.bicep' = {
  name: '${version}-sg'
  scope: resourceGroup(rgName)
  params: {
    name: sgName    
    queues: 'sample-work'
    blobContainers: 'sample-work'
    tables: 'samplework'
    storageType: envType[environment]
  }
  dependsOn: [
    rg
  ]
}


module appInsights 'appinsights/template.bicep' = {
  name: '${version}-ins'
  scope: resourceGroup(rgName)
  params: {
    name: appInsName
  }
  dependsOn: [
    rg
  ]
}

module appServicePlan 'appserviceplan/template.bicep' = {
  name: '${version}-asp'
  scope: resourceGroup(rgName)
  params: {
    name: aspName
    category: envType[environment]
  }
  dependsOn: [
    rg
  ]
}

module keyVault 'keyvault/template.bicep' = {
  name: '${version}-kv'
  scope: resourceGroup(rgName)
  params: {
    name: kvName
  }
  dependsOn: [
    rg
  ]
}

module app 'functionapp/template.bicep' = {
  name: '${version}-fn'
  scope: resourceGroup(rgName)
  params: {
    appName: funcAppName
    aspName: aspName
  }
  dependsOn: [
    appServicePlan
  ]
}

module kvPolicies 'keyvault/policies.bicep' = {
  scope: resourceGroup(rgName)
  name: '${version}-kv-policies'
  params: {
    appId: app.outputs.prodId
    appInsightsName: appInsName
    kvName: kvName
    storageName: sgName
  }
  dependsOn: [
    app
    appInsights
    keyVault
    storageAccount
  ]
}

module rbacSetting 'rbac/template.bicep' = {
  scope: resourceGroup(rgName)
  name: '${version}-rbac'
  params: {
    appId: app.outputs.prodId
    friendlyName: funcAppName
    storageName: sgName
  }
  dependsOn: [
    app
    storageAccount    
  ]
}

module funcAppSettings 'functionapp/configurations.bicep' = {
  scope: resourceGroup(rgName)
  name: '${version}-fn-settings'
  params: {
    appName: funcAppName
    kvName: kvName
    storageName: sgName
  }
  dependsOn: [
    app
    keyVault
    rbacSetting
  ]
}
