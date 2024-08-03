targetScope='subscription'


@description('Name of the resource group')
param resourceGroupName string

@description('Location where the resource group will be created')
param resourceGroupLocation string

resource newRg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
}