param location string = resourceGroup().location

param githubRepo string = 'celloza/azure-keyvault-ca-portal'
param appTitle string
param vwanHubName string
param vwanHubRg string

var suffix = uniqueString(resourceGroup().id)

// Role Lookup Module (Dynamic ID fetching)
module roleLookup 'modules/role_lookup.bicep' = {
  name: 'deploy-role-lookup'
  params: {
    location: location
  }
}

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
    storageTableRoleDefinitionId: roleLookup.outputs.storageTableRoleDefinitionId
    storageBlobRoleDefinitionId: roleLookup.outputs.storageBlobRoleDefinitionId
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
    keyVaultAdminRoleDefinitionId: roleLookup.outputs.keyVaultAdminRoleDefinitionId
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
