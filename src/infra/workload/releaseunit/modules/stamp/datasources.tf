Skip to content
Search or jump to…
Pull requests
Issues
Codespaces
Marketplace
Explore
 
@cberon-costco 
Azure
/
Mission-Critical-Connected
Public template
Fork your own copy of Azure/Mission-Critical-Connected
Code
Issues
1
Pull requests
1
Actions
Projects
Security
Insights
Mission-Critical-Connected/src/infra/workload/releaseunit/modules/stamp/datasources.tf
@heoelri
heoelri Add missing data contributor role (#480)
…
Latest commit 78fefa0 3 days ago
 History
 3 contributors
@sebader@heoelri@dependabot
 50 lines (40 sloc)  1.58 KB

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
Footer
© 2023 GitHub, Inc.
Footer navigation
Terms
Privacy
Security
Status
Docs
Contact GitHub
Pricing
API
Training
Blog
About
Mission-Critical-Connected/datasources.tf at main · Azure/Mission-Critical-Connected