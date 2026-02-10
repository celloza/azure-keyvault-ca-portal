param location string
param suffix string
param snetPeId string
param pdnsBlobId string
param appIdentityPrincipalId string

resource st 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'stcamanager${suffix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Disabled'
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: st
  name: 'default'
}

resource auditTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: tableService
  name: 'auditlogs'
}

// Private Endpoint
resource peBlob 'Microsoft.Network/privateEndpoints@2023-04-01' = {
  name: 'pe-st-blob'
  location: location
  properties: {
    subnet: {
      id: snetPeId
    }
    privateLinkServiceConnections: [
      {
        name: 'psc-st-blob'
        properties: {
          privateLinkServiceId: st.id
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

resource peBlobDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-04-01' = {
  parent: peBlob
  name: 'pdns-blob-group'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-blob-core-windows-net'
        properties: {
          privateDnsZoneId: pdnsBlobId
        }
      }
    ]
  }
}

// RBAC
resource roleStorageBlobDataContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource roleStorageTableDataContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

resource roleAssignmentBlob 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: st
  name: guid(st.id, appIdentityPrincipalId, roleStorageBlobDataContributor.id)
  properties: {
    roleDefinitionId: roleStorageBlobDataContributor.id
    principalId: appIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource roleAssignmentTable 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: st
  name: guid(st.id, appIdentityPrincipalId, roleStorageTableDataContributor.id)
  properties: {
    roleDefinitionId: roleStorageTableDataContributor.id
    principalId: appIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output storageAccountName string = st.name
