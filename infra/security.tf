resource "azurerm_user_assigned_identity" "app_identity" {
  name                = "id-camanager"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_key_vault" "kv" {
  name                          = "kv-camanager-${random_string.suffix.result}"
  location                      = azurerm_resource_group.rg.location
  resource_group_name           = azurerm_resource_group.rg.name
  tenant_id                     = data.azurerm_client_config.current.tenant_id
  sku_name                      = "standard"
  rbac_authorization_enabled    = true
  public_network_access_enabled = false
  soft_delete_retention_days    = 7
  purge_protection_enabled      = false
}

resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_private_endpoint" "pe_kv" {
  name                = "pe-kv-camanager"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  subnet_id           = azurerm_subnet.snet_pe.id

  private_service_connection {
    name                           = "psc-kv"
    private_connection_resource_id = azurerm_key_vault.kv.id
    subresource_names              = ["vault"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "pdns-kv-group"
    private_dns_zone_ids = [azurerm_private_dns_zone.pdns_vault.id]
  }
}

resource "azurerm_role_assignment" "kv_admin" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = azurerm_user_assigned_identity.app_identity.principal_id
}

data "azurerm_client_config" "current" {}
