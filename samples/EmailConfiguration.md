# Email Configuration 
This page contains information about the `email` command configuration files.
The `email` command supports both YAML and JSON formats for the configuration.

- YAML sample configuration: [sample-email-config.yml](./sample-email-config.yml)
- JSON sample configuration: [sample-email-config.json](./sample-email-config.json)

## Configuration file structure

- `SmtpConfig` [object(SmtpConfig)] **required** : SMTP server configuration (see **SmtpConfig** below).
- `FromAddress` [string] **required** : From email address.
- `FromLabel` [string] *optional* : Optional From display name (label).
- `ToAddress` [string] **required** : To email address.
- `MessageSubject` [string] *optional* : Email message subject.
- `MessageBody` [string] **required** : Email message body (in plain text or HTML format). Can be either an inline string, or path to a file containing the string. 
- `ReadBodyFromFile` [boolean] *optional* : If set to **TRUE**, `MessageBody` must be the name of the file containing the body (with absolute or relative path), otherwise an inline string will be expected (default false).

**SmtpConfig**:
- `Host` [string] **required** : SMTP server host.
- `Port` [integer] **required** : SMTP port (default 25).
- `User` [string] *optional* : SMTP server username (if authentication is required).
- `Password` [string] *optional* : SMTP server password (if authentication is required). 
- `UseSsl` [boolean] *optional* : If **TRUE** SSL/TSL will be used (default false).
- `Timeout` [integer] *optional* : SMTP connection timeout in milliseconds (default 30000).
