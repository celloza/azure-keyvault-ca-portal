param location string = resourceGroup().location
param utcValue string = utcNow()

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-deployment-script'
  location: location
}

// Assign Reader role to the Managed Identity so it can list Role Definitions
// Reader Role ID: acdd72a7-3385-48ef-bd42-f606fba81ae7
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, uami.id, 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'acdd72a7-3385-48ef-bd42-f606fba81ae7'
    )
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource lookupScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'ds-lookup-roles'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uami.id}': {}
    }
  }
  properties: {
    azCliVersion: '2.50.0'
    retentionInterval: 'P1D'
    forceUpdateTag: utcValue // Ensure it runs if re-deployed
    scriptContent: '''
      echo "Looking up Role IDs..."
      
      STORAGE_TABLE_ID=$(az role definition list --name "Storage Table Data Contributor" --query "[0].name" -o tsv)
      STORAGE_BLOB_ID=$(az role definition list --name "Storage Blob Data Contributor" --query "[0].name" -o tsv)
      KV_ADMIN_ID=$(az role definition list --name "Key Vault Administrator" --query "[0].name" -o tsv)

      # Handle cases where roles might not be found (fallback or error)
      if [ -z "$STORAGE_TABLE_ID" ]; then echo "Error: Storage Table role not found"; exit 1; fi
      if [ -z "$STORAGE_BLOB_ID" ]; then echo "Error: Storage Blob role not found"; exit 1; fi
      if [ -z "$KV_ADMIN_ID" ]; then echo "Error: Key Vault Admin role not found"; exit 1; fi

      # Output JSON
      echo "{\"storageTableRoleDefinitionId\": \"$STORAGE_TABLE_ID\", \"storageBlobRoleDefinitionId\": \"$STORAGE_BLOB_ID\", \"keyVaultAdminRoleDefinitionId\": \"$KV_ADMIN_ID\"}" > $AZ_SCRIPTS_OUTPUT_PATH
    '''
  }
  dependsOn: [
    roleAssignment // Wait for permission assignment
  ]
}

output storageTableRoleDefinitionId string = lookupScript.properties.outputs.storageTableRoleDefinitionId
output storageBlobRoleDefinitionId string = lookupScript.properties.outputs.storageBlobRoleDefinitionId
output keyVaultAdminRoleDefinitionId string = lookupScript.properties.outputs.keyVaultAdminRoleDefinitionId
