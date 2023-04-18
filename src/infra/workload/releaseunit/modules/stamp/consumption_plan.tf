resource "azurerm_consumption_budget_resource_group" "regional_rg_budget" {
  name       = "${azurerm_resource_group.stamp.name}_budget"
  amount     = 100
  time_grain = "Monthly" // Monthly, Quarterly, Annually
  time_period {
    start_date = formatdate("2023-04-01'T'00:00:00Z",timestamp()) //Budget needs to be first day of current month for start
  }
  resource_group_id = azurerm_resource_group.stamp.id
  notification {
    threshold      = 80                     // 0-1000
    operator       = "GreaterThanOrEqualTo" // EqualTo, GreaterThan, GreaterThanOrEqualTo
    contact_emails = ["thinguyen01@costco.com"]
  }
  lifecycle {
    ignore_changes = [
      time_period
    ]
  }
}