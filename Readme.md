# SoupDiscover
Cette application permet d'extraire l'ensemble des packages ainsi que les métadonnées associées.
Elle recherche les paquets Nuget et NPM.

// Ajouter une capture d'écran de l'application

# Compilation et executation de l'application

## Avec Docker
```
docker build . -t soupdicover:latest
docker run -d -P soupdicover:latest
```

## En local
```
dotnet build SoupDiscover.csproj
dotnet ServerAndAngular\bin\Debug\netcoreapp3.1\SoupDiscover.dll
```

# Variables d'environement

Si l'application est exécuté dans un docker, ele peut être paramétrée par l'intermédiaire de varaibles d'environement.

| Nom de la variable | Description | Valeur par défaut |
|---|---|---|
| __TempWork__ | Le dossier dans lequel déployer les dépot | Le dossier temporaire de l'OS |
| __ConnectionString__ | Chaine de connection d'accès à la base de données | `Data Source=CustomerDB.db`  |
| __DatabaseType__  | Le type de base de données. Les type de base de données supportées sont "SQLite" et "Postgres" | `SQLite` |

# Paramétrages de appsetting.json
:construction_worker: