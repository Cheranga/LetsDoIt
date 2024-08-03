targetScope = 'subscription'

param appName string
param version string
param location string

@allowed([
  'dev'
  'qa'
])
param environment string

var appNameWithEnvironment = '${appName}-${environment}'
var rgName = 'cchat-rg-${appNameWithEnvironment}'

module rg 'resourcegroup/template.bicep' = {
  scope: subscription()
  name: '${version}-rg'
  params: {
    location: location
    name: rgName
  }
}
