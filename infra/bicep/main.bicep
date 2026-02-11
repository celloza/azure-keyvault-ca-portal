param location string = resourceGroup().location

param githubRepo string = 'celloza/azure-keyvault-ca-portal'
param appTitle string
param vwanHubName string
param vwanHubRg string

var suffix = uniqueString(resourceGroup().id)

// Role Definition IDs (Defaults are standard Azure IDs)
// Users with non-standard IDs must override these parameters
param storageTableRoleDefinitionId string = '0a9a7e1f-b9d0-4cc4-a60d-0319cd74161d'
param storageBlobRoleDefinitionId string = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
param keyVaultAdminRoleDefinitionId string = '00482a5a-887f-4fb3-b363-3b7fe8e74483'

module network 'modules/network.bicep' = {
  name: 'deploy-network'
  params: {
    location: location
  }
}

module vwan 'modules/vwan.bicep' = {
  name: 'deploy-vwan'
  scope: resourceGroup(vwanHubRg)
  params: {
    vnetId: network.outputs.vnetId
    vwanHubName: vwanHubName
  }
}

module identity 'modules/identity.bicep' = {
  name: 'deploy-identity'
  params: {
    location: location
  }
}

module logging 'modules/logging.bicep' = {
  name: 'deploy-logging'
  params: {
    location: location
    suffix: suffix
  }
}

module storage 'modules/storage.bicep' = {
  name: 'deploy-storage'
  params: {
    location: location
    suffix: suffix
    snetPeId: network.outputs.snetPeId
    pdnsBlobId: network.outputs.pdnsBlobId
    appIdentityPrincipalId: identity.outputs.principalId
    storageTableRoleDefinitionId: storageTableRoleDefinitionId
    storageBlobRoleDefinitionId: storageBlobRoleDefinitionId
  }
}

module keyvault 'modules/keyvault.bicep' = {
  name: 'deploy-keyvault'
  params: {
    location: location
    suffix: suffix
    snetPeId: network.outputs.snetPeId
    pdnsVaultId: network.outputs.pdnsVaultId
    appIdentityPrincipalId: identity.outputs.principalId
    tenantId: subscription().tenantId
    keyVaultAdminRoleDefinitionId: keyVaultAdminRoleDefinitionId
  }
}

module appService 'modules/appservice.bicep' = {
  name: 'deploy-appservice'
  params: {
    location: location
    suffix: suffix
    snetAppId: network.outputs.snetAppId
    snetPeId: network.outputs.snetPeId
    pdnsAppId: network.outputs.pdnsAppId
    appInsightsConnectionString: logging.outputs.appInsightsConnectionString
    tenantId: subscription().tenantId
    githubRepo: githubRepo
    appTitle: appTitle
    identityId: identity.outputs.id
    identityClientId: identity.outputs.clientId
    keyVaultUrl: keyvault.outputs.vaultUri
    storageAccountName: storage.outputs.storageAccountName
    auditTableName: 'auditlogs'
  }
}
