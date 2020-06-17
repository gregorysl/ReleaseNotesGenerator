# ReleaseNotesGenerator

Application to generate release notes documents based on changes and work items done in TFS or Azure DevOps

## Settings

name | type | description
---- | ---- | ---- 
Data | ReleaseData | General information about release
ChangesetsServer | ServerDetails | Details of server to download changes
WorkItemServer | ServerDetails | Details of server to download Work Items
DocumentLocation | string | Location for Template.docx, and output location for generated document
TestReport | string | Url to additional Sign-off document
WorkItemStateInclude | List<string> | List of Work Item Types to include in document

# ReleaseData settings
name | type | description
---- | ---- | ---- 
TfsProject | string | Project to take data from
TfsBranch | string | TFS/Git branch to take data from
Iteration | string | Iteration path to use
ChangesetFrom | string | Changeset Id/SHA-1 of earliest commit we want to include in document
ChangesetTo | string | Changeset Id/SHA-1 of latest commit we want to include in document
ReleaseName | string | Name of release
ReleaseDate | string | Date of release
QaBuildName | string | Name of QA build used to test release
QaBuildDate | string | Date of QA build used to test release

# ServerDetails settings
name | type | description
---- | ---- | ---- 
Url | string | Server link  in following format `https://{instance}[/{team-project}]`
Pat | string | Personal Access Token granting access to server
ServerType | ServerTypeEnum | type of server. Currently supported Tfs/Azure

There are two token replacements set up:

- {ReleaseName}
- {ReleaseDate}

If you add them to your docx template, they'll be replaced in end document for actual values
