variable "location" {
  description = "Azure Region for this stamp"
  type        = string
}

variable "prefix" {
  description = "Resource Prefix"
  type        = string
}

variable "suffix" {
  description = "A suffix used for all resources in this example. Must not contain any special characters. Must not be longer than 10 characters."
  type        = string
  default     = ""
}

variable "queued_by" {
  description = "Name of the user who has queued the pipeline run that has deployed this environment. Used as an Azure Resource Tag."
  type        = string
  default     = "n/a"
}

variable "default_tags" {}

variable "vnet_resource_id" {
  description = "VNet resource IDs for this stamp"
  type        = string
}

variable "custom_dns_zone" {
  description = "Custom DNS Zone name"
  type        = string
}

variable "custom_dns_zone_resourcegroup_name" {
  description = "Resource Group Name of the Custom DNS Zone"
  type        = string
}

variable "acr_name" {
  description = "Azure Container Registry name used for AcrPull role assignment"
  type        = string
}

variable "azure_monitor_action_group_resource_id" {
  description = "Resource ID of a Azure Monitor action group to send alerts to"
  type        = string
}

variable "global_resource_group_name" {
  description = "Name of the resource group where global resources (like Cosmos DB and ACR) live if"
  type        = string
}

variable "monitoring_resource_group_name" {
  description = "Name of the resource group which holds the shared monitoring resources"
  type        = string
}

variable "cosmosdb_account_name" {
  description = "Account name of the Cosmos DB"
  type        = string
}

variable "cosmosdb_database_name" {
  description = "Name of the globally shared cosmos db database"
  type        = string
}

variable "aks_kubernetes_version" {
  description = "Kubernetes Version"
  type        = string
}

variable "alerts_enabled" {
  description = "Enable alerts?"
  type        = bool
}

variable "aks_system_node_pool_sku_size" {
  description = "VM SKU of the AKS system nodes"
  type        = string
}

variable "aks_system_node_pool_autoscale_minimum" {
  description = "Minimum number of AKS system nodes for auto-scale settings"
  type        = number
}

variable "aks_system_node_pool_autoscale_maximum" {
  description = "Maximum number of AKS system nodes for auto-scale settings"
  type        = number
}

variable "aks_user_node_pool_sku_size" {
  description = "VM SKU of the AKS worker nodes"
  type        = string
}

variable "aks_user_node_pool_autoscale_minimum" {
  description = "Minimum number of AKS worker nodes for auto-scale settings"
  type        = number
}

variable "aks_user_node_pool_autoscale_maximum" {
  description = "Maximum number of AKS worker nodes for auto-scale settings"
  type        = number
}

variable "event_hub_thoughput_units" {
  description = "Number of Throughput Units for Event Hub Namespace"
  type        = number
}

variable "event_hub_enable_auto_inflate" {
  description = "Enable auto-inflate of TUs for Event Hub Namespace?"
  type        = bool
}

variable "event_hub_auto_inflate_maximum_tu" {
  description = "Auto-inflate maximum TUs for Event Hub Namespace. Only applies if event_hub_enable_auto_inflate=true"
  type        = number
}

variable "global_storage_account_name" {
  description = "Name of the globally shared storage account, which is used for image storage"
  type        = string
}

variable "api_key" {
  description = "API Key for protecting sensitive APIs in the CatalogService"
  type        = string
  sensitive   = true
}

variable "ai_adaptive_sampling" {
  description = "Enable adaptive sampling in Application Insights. Setting this to false means that 100% of the telemetry will be collected."
  type        = bool
  default     = true
} 

variable "redis_cache_name" {
  description = "Global name for Redis Cache Service."
  type        = string
}

variable "redis_cache_id" {
  description = "Id of global for Redis Cache Service."
  type        = string
}