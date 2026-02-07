variable "resource_group_name" {
  description = "Name of the Resource Group"
  type        = string
  default     = "rg-camanager-dev"
}

variable "location" {
  description = "Azure Region"
  type        = string
  default     = "Start"
}

variable "github_repo" {
  description = "GitHub Repository (org/repo)"
  type        = string
  default     = "celloza/azure-keyvault-ca-portal"
}

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}

variable "app_title" {
  description = "Title of the Application"
  type        = string
  default     = "CaManager"
}
