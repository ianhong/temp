data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "vnet_rg" {
  name = local.vnet_resource_group_name
}

data "azurerm_virtual_network" "stamp" {
  name                = local.vnet_name
  resource_group_name = local.vnet_resource_group_name
}

data "azurerm_cosmosdb_account" "global" {
  name                = var.cosmosdb_account_name
  resource_group_name = var.global_resource_group_name
}

data "azurerm_container_registry" "global" {
  name                = var.acr_name
  resource_group_name = var.global_resource_group_name
}

data "azurerm_log_analytics_workspace" "stamp" {
  name                = "${local.global_resource_prefix}-${local.location_short}-log"
  resource_group_name = var.monitoring_resource_group_name
}

data "azurerm_application_insights" "stamp" {
  name                = "${local.global_resource_prefix}-${local.location_short}-appi"
  resource_group_name = var.monitoring_resource_group_name
}

data "azurerm_storage_account" "global" {
  name                = var.global_storage_account_name
  resource_group_name = var.global_resource_group_name
}

data "azurerm_redis_cache" "global" {
  name                = var.redis_cache_name
  resource_group_name = var.global_resource_group_name
}

data "azurerm_resource_group" "buildagent" {
  name = "${var.prefix}-buildinfra-rg"
}

data "azurerm_virtual_network" "buildagent" {
  name                = "${var.prefix}buildinfra-vnet"
  resource_group_name = data.azurerm_resource_group.buildagent.name
}

data "azurerm_cosmosdb_sql_role_definition" "builtin_data_contributor" {
  resource_group_name = var.global_resource_group_name
  account_name        = data.azurerm_cosmosdb_account.global.name
  role_definition_id  = "00000000-0000-0000-0000-000000000002"
}