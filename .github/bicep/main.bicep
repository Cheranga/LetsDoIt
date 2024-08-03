targetScope = 'subscription'

param appName string
param version string


@allowed([
  'dev'
  'qa'
])
param environment string

var envType = {
    dev: 'nonprod',
    qa: 'nonprod',
    prod: 'prod'
    } 

var location = resourceGroup().location
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
    location: location
    queues: 'inputs'
    blobContainers: 'input'
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
    location: location
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
    location: location
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
    location: location
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
    location: location
  }
  dependsOn: [
    rg
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
    rg
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
    rg
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