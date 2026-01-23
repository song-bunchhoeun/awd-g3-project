# CI/CD Workflows

This directory contains the documentation and structure for the GitHub Actions workflows used to build and deploy the **DGC.eKYC.Api** project to Azure Web Apps using **Azure Managed Identity (OIDC)** and **GitHub Environments**.

We follow a modular workflow structure to separate concerns between Continuous Integration (CI), Continuous Deployment (CD), and reusable deployment logic.

## Proposed Workflow Structure

The team should implement the following YAML files in this directory:

| File | Purpose | Description |
|------|---------|-------------|
| **`ci.yaml`** | **C**ontinuous **I**ntegration | Triggers on PRs and pushes to `main`. Builds the .NET project and runs tests. |
| **`cd.yaml`** | **C**ontinuous **D**eployment | Triggers after CI or on specific tags. Orchestrates deployment by selecting the correct environment. |
| **`_deploy.yaml`** | Reusable Workflow | A shared template that builds and deploys to a specific Azure Web App environment using Managed Identity. |

---

## Implementation Guide

### 1. CI Workflow (`ci.yaml`)

This workflow ensures code quality by building and testing every change.

```yaml
name: CI

on:
  push:
    branches: [ "master", "uat", "dev" ]
  pull_request:
    branches: [ "master", "uat", "dev" ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### 2. Reusable Deployment Workflow (`_deploy.yaml`)

This reusable workflow handles the build and deployment logic using Azure Managed Identity. It uses `vars.AZURE_WEBAPP_NAME` which is automatically resolved based on the `environment` context.

```yaml
name: Reusable Deploy

on:
  workflow_call:
    inputs:
      environment:
        required: true
        type: string
        description: 'Environment to deploy to (dev, staging, prod)'
      client_id:
        required: true
        type: string
        description: 'Azure User Assigned Identity Client ID'
      tenant_id:
        required: true
        type: string
        description: 'Azure Tenant ID'
      subscription_id:
        required: true
        type: string
        description: 'Azure Subscription ID'

permissions:
  id-token: write
  contents: read

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    environment: ${{ inputs.environment }}
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Build and Publish
        run: dotnet publish DGC.eKYC.Api/DGC.eKYC.Api.csproj -c Release -o ./publish

      - name: Azure Login
        uses: azure/login@v2
        with:
          client-id: ${{ inputs.client_id }}
          tenant-id: ${{ inputs.tenant_id }}
          subscription-id: ${{ inputs.subscription_id }}

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          # This variable comes from the GitHub Environment configuration
          app-name: ${{ vars.AZURE_WEBAPP_NAME }}
          package: ./publish
```

### 3. CD Workflow (`cd.yaml`)

This workflow determines the target environment based on the branch name and then calls the reusable workflow.

```yaml
name: CD

on:
  push:
    branches: [ "master", "uat", "dev" ]

jobs:
  config:
    runs-on: ubuntu-latest
    outputs:
      environment: ${{ steps.set-env.outputs.environment }}
      git_sha: ${{ steps.vars.outputs.git_sha }}
    steps:
      - id: vars
        run: echo "git_sha=${{ github.sha }}" >> $GITHUB_OUTPUT

      - id: set-env
        run: |
          # Map branches to GitHub Environments
          if [[ "${{ github.ref_name }}" == "master" ]]; then
            ENV_NAME="production"
          elif [[ "${{ github.ref_name }}" == "uat" ]]; then
            ENV_NAME="uat"
          else
            ENV_NAME="dev"
          fi

          echo "Determined Environment: $ENV_NAME"
          echo "environment=$ENV_NAME" >> $GITHUB_OUTPUT

  debug-outputs:
    needs: config
    runs-on: ubuntu-latest
    steps:
    - id: debug-context
      run: |
        echo "### Triggering Event"                                 >> $GITHUB_STEP_SUMMARY
        echo "| Var | Value |"                                      >> $GITHUB_STEP_SUMMARY
        echo "|:--|:--|"                                            >> $GITHUB_STEP_SUMMARY
        echo "| event_name      | ${{ github.event_name }} |"       >> $GITHUB_STEP_SUMMARY
        echo "| ref_name        | ${{ github.ref_name }} |"         >> $GITHUB_STEP_SUMMARY

        echo "### Pipeline Variables"                                         >> $GITHUB_STEP_SUMMARY
        echo "| Var | Value |"                                                >> $GITHUB_STEP_SUMMARY
        echo "|:--|:--|"                                                      >> $GITHUB_STEP_SUMMARY
        echo "| Target Environment | ${{ needs.config.outputs.environment }}" >> $GITHUB_STEP_SUMMARY
        echo "| Git sha | ${{ needs.config.outputs.git_sha }}"                >> $GITHUB_STEP_SUMMARY

  deploy:
    needs: [config, debug-outputs]
    uses: ./.github/workflows/_deploy.yaml
      with:
      environment: ${{ needs.config.outputs.environment }}
      client_id: ${{ secrets.AZURE_CLIENT_ID }}
      tenant_id: ${{ secrets.AZURE_TENANT_ID }}
      subscription_id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

## Setup Requirements

To make this work, you must configure **GitHub Environments** and **Variables**:

1.  **Configure GitHub Environments**:
    *   Go to Repository Settings > **Environments**.
    *   Create three environments: `dev`, `uat`, and `production`.

2.  **Add Environment Variables**:
    *   For **each environment** (click on the environment name in settings):
    *   Add a new variable (Variables > Environment variables):
        *   **Name**: `AZURE_WEBAPP_NAME`
        *   **Value**: The name of the Azure App Service for that environment (e.g., `dgc-ekyc-api-dev` for `dev`).

3.  **Configure Azure OIDC (Managed Identity)**:
    *   **Create User Assigned Managed Identity** in Azure.
    *   **Assign `Contributor`** role on the Resource Group.
    *   **Add Federated Credentials** for each environment (`dev`, `uat`, `production`) matching the GitHub environments.

4.  **Configure Repository Secrets**:
    *   Go to Repository Settings > Secrets and variables > Actions.
    *   Add `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID`.
