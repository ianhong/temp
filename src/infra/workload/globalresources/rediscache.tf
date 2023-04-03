resource "azurerm_redis_cache" "global" {
  name                = "${local.prefix}-global-redis"
  resource_group_name = azurerm_resource_group.global.name
  location            = azurerm_resource_group.global.location
  capacity            = var.redis_cache.capacity
  family              = var.redis_cache.family
  sku_name            = var.redis_cache.sku
  enable_non_ssl_port = false
  public_network_access_enabled = false
  minimum_tls_version = "1.2"

  redis_configuration {}
}
