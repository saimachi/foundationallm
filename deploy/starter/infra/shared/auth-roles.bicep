param authStoreName string
param keyvaultName string
param identityId string
param principalId string

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource secretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault
  name: guid(subscription().id, resourceGroup().id, identityId, 'secretsUserRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalType: 'ServicePrincipal'
    principalId: principalId
  }
}

resource secretsAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyvault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: principalId
        permissions: { secrets: [ 'get', 'list' ] }
        tenantId: subscription().tenantId
      }
    ]
  }
}

resource authStore 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: authStoreName
}

resource authStoreRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: authStore
  name: guid(subscription().id, resourceGroup().id, identityId, 'blobAdmin')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
    principalType: 'ServicePrincipal'
    principalId: principalId
  }
}

