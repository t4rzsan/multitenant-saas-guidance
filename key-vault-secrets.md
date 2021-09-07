# Setting up Key Vault in the Surveys app

This doc shows how to store application secrets for the Surveys app in Azure Key Vault.

Prerequisites:

- Configure the Surveys application as described [here](./get-started.md).

> To create a key vault, you must use an account which can manage your Azure subscription. Also, any application that you authorize to read from the key vault must be registered in the same tenant as that account.

## Move the ClientSecret to key vault

- [Create a key vault using Azure Portal](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal#create-a-vault)

- Assign the Identity to access Key Vault. We are going to use [Manage Identity](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0#use-managed-identities-for-azure-resources)

  - Navigate the Key Vault
  - Navigate Access Policies
  - Add Access Policy
  - Select List and Get Secret
  - Select principal
    - Running locally, the same account which you are log-in on Visual Studio
    - Running on App Service, the Object ID. The Object ID is shown in the Azure portal on the Identity panel of the App Service.
  - Save

- Create secret
  - Navigate Secret
  - Create secret
  - The secret name must be **AzureAd--ClientSecret**
  - Take the value from **ClientSecret** on the secret file configure when started the app [here](./get-started.md).
  - Delete the **ClientSecret** from the secret file, it is going to be on Key Vault now

## Change code

On Tailspin.Surveys.Web, Program.cs, the following code need to uncomment

```dotnetcli
.ConfigureAppConfiguration((context, config) =>
                {
                        var builtConfig = config.Build();
                        var secretClient = new SecretClient(
                            new Uri($"https://{builtConfig["KeyVaultName"]}.vault.azure.net/"),
                            new DefaultAzureCredential());
                        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                });
```

On the app setting we need to configure the following value

```dotnetcli
 "KeyVaultName"
```

## Execute

The app must continue working on the same way, but now the secret is coming from Key Vault by Manage Identity
