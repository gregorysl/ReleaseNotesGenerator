# ReleaseNotesGenerator

Application to generate release notes documents based on changes and work items done in TFS or Azure DevOps

## Settings

name | type | required/optional | description
---- | ---- | ---- | ---- 
Data | ReleaseData | required | General information about release
ChangesetsServer | ServerDetails | required | Details of server to download changes
WorkItemServer | ServerDetails | required | Details of server to download Work Items
DocumentLocation | string | reuired | Location for Template.docx, and output location for generated document
TestReport | string | optional | Url to additional Sign-off document
WorkItemStateInclude | List<string> | optional | List of Work Item Types to include in document

# ReleaseData settings
name | type | required/optional | description
---- | ---- | ---- | ---- 
TfsProject | string | required | Project to take data from
TfsBranch | string | required | TFS/Git branch to take data from
Iteration | string | required | Iteration path to use
ChangesetFrom | string | optional | Changeset Id/SHA-1 of earliest commit we want to include in document
ChangesetTo | string | optional | Changeset Id/SHA-1 of latest commit we want to include in document
ReleaseName | string | optional | Name of release
ReleaseDate | string | optional | Date of release
QaBuildName | string | optional | Name of QA build used to test release
QaBuildDate | string | optional | Date of QA build used to test release

# ServerDetails settings
name | type | required/optional | description
---- | ---- | ---- | ---- 
Url | string | required | Server link  in following format `https://{instance}[/{team-project}]`
Pat | string | required | Personal Access Token granting access to server
ServerType | ServerTypeEnum | required | type of server. Currently supported Tfs/Azure

There are two token replacements set up:

- {ReleaseName}
- {ReleaseDate}

If you add them to your docx template, they'll be replaced in end document for actual values
