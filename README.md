# ReleaseNotesGenerator

Application to generate release notes documents based on changes done in TFS with work items kept in Azure DevOps.

## Settings

### Data

- **TfsProject**: Project in TFS for source of changes
- **TfsBranch**: Branch in TFS Project for source of changes
- **Iteration**: Azure iteration, source of Work Items
- **ChangesetFrom**: starting number for release changes
- **ChangesetTo**: ending number for release changes
- **ReleaseName**: Name of release
- **ReleaseDate**: Date of release
- **QaBuildName**: Name of build used for testing release
- **QaBuildDate**: Date of build used for testing release

### Tfs/Azure

- **Url**: Url to Azure/TFS server in following format `https://{instance}[/{team-project}]`
- **Pat**: Personal Access Token

### Other

- **DocumentLocation**: input location for Template.docx, and output location for generated document
- **WorkItemStateInclude**: array of work items states that should be considered as completed in release

There are two token replacements set up:

- {ReleaseName}
- {ReleaseDate}

If you add them to your docx template, they'll be replaced in end document for actual values
