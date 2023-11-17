# DevopsPermissionsADO

## Introduction

The goal of this project is to provide a way to list permissions in Azure DevOps. This project is based on the [Azure DevOps REST API](https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-5.1).

## Getting Started

Donwload the project and open it in Visual Studio. You can also use Visual Studio Code.

### Prerequisites

Personal Access Token (PAT) with the following permissions:
- Read

### Notes

Not all permissions will be listed in the output because some are internal to Azure DevOps, and only those listed by Namespaces can be documented.
You still can get all permissions but you will not know which namespace they belong to.
