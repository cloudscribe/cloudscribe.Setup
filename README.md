# cloudscribe.Setup

cloudscribe.Setup provides a framework for running intallation and upgrade scripts for ADO.NET data implementations in a web application. The [ADO.NET data layers for cloudscribe Core](https://github.com/joeaudette/cloudscribe.Core.Data) are designed by convention to use this setup system. cloudscribe.Setup includes a SetupController for ASP.NET Core that automates the process of detecting and executing new installation and upgrade scripts.

[![Join the chat at https://gitter.im/joeaudette/cloudscribe](https://badges.gitter.im/joeaudette/cloudscribe.svg)](https://gitter.im/joeaudette/cloudscribe?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

The /Setup page can run sql scripts found under

/config/applications/[applicationname]/install/[dbplatform]/

and under

/config/applications/[applicationname]/upgrade/[dbplatform]/

where "config" is the name of a root level folder in the web application project

It will start with the install folder, from that folder it will run only one script, it will choose the highest version script found in the folder, and it only does this if there is no existing row for the application in mp_SchemaVersion table for the given 'applicationname'. After a row exists it will only run higher version scripts from the upgrade folder.

After running a script it will log the application name and script version in mp_SchemaVersion
once a log record is established it will never run any script unless it is newer version than the logged version
if it finds higher version script(s) in the upgrade folder, it will run them in version order and then update the version in mp_SchemaVersions after each script.

In some cases you may want to prevent it from running newer scripts. For example you may still be developing the script for the next version but the file exists on disk. Normally the setup page would execute that script and we need a way to tell it what script to stop at so that it won't execute the new script until we are ready.

Our current solution is to provide a provider model for VersionProvider, the base class is

cloudscribe.Configuration.VersionProvider.cs

cloudscribe core implements a provider and it is plugged in by xml configuration under 

/config/CodeVersionProviders

The implementation code for cloudscribe.Core is in cloudscribe.Core.Models.CloudscribeCoreVersionProvider.cs
and that file is the one to update (ie increment the version) when we are ready for the setup page to execute the new script.

The setup page will look for a provider name that matches the application name, if it finds one, it will use that to determine the current code version.
If it a matching version provider is found, whatever the code version it reports, then the setup page will not run scripts higher than that version for the given application.

In the absence of a matching VersionProvider, the setup page will run any higher version script it finds in the upgrade folder.

You can use the setup system and setup page for your own scripts simply by creating a folder for your applicationname (don't use spaces or special characters in the folder name, lowercase is recomended) and sub folders with similarly versioned sql files for whichever db platforms you care to support.
Optionally you can also implement a CodeVersionProvider as discussed above.

Of course if we later implement repositories using Entity Framework or if you build your own applications using Entity Framework then you would not use this setup system or sql scripts for that but would instead use the migrations strategy supported by EF when making changes to the models.

### About Versions

Versions are based on the System.Version using major.minor.build.revision as a mechanism to simply increment install/upgrade scripts.

The idea is for the code version of the application to match the schema version of the db, so in the file system the upgrade scripts would exist on disk named after the version:
* 1.0.0.1.sql
* 1.0.0.2.sql
* ..
* 1.0.0.9.sql
* 1.0.1.0.sql
* ...
* using simple incrementing of single digit per segment to increase the version.

So whenever you want to make changes to the db schema (presumably corresponding to changes in the code),
you add a new script with the next higher version increment and if you have a versionprovider implementation you increment it to make the code version matches the new script.

Since we don't want to run out of versions we generally do not jump up version numbers i.e. we don't suddenly go from 1.0.0.1 to 2.0.0.0, we only increment one digit at a time. Though you could make skips and jumps in version if you want to.

So you may in some cases want to have a completely separate version number to identify your application/product for branding/marketing purposes to mark major milestones. That is a business decision whether you want to have a separate version for branding purposes that does not correspond to the schema and code versions which are more technical in nature and whose main purpose is to facilitate use of the setup system.

For our own cloudscribe applications we choose to just use our schema/code versions which just increment simply as mentioned, and we want to communicate to our customers that not much interpretation should be based on our version numbers. ie a small increment in the version could be a major update or a minor update. The release notes should convey whether it is a minor or major upgrade.

### Why Not Use One Folder for both install and upgrade?

The install folder is treated differently, only if the application has no schema version at all will it run the highest version script from the install folder, so only during a new installation. You may start with version 1.0.0.0.config in the install folder then after many incremental upgrades say you get to version 2.0.0.0 there will be numerous scripts in the upgrade folder to run for a new installation and you "could" create a new install script that has everything needed for version 2.0.0.0 in a single script. Since it would be the highest script in the install folder, new installations could then start with the 2.0.0.0.config script instead of running so many upgrade scripts in a row. Then it would only run scripts newer than 2.0.0.0 from the upgrade folder. So this approach makes it possible to condense the latest schema into a newer install script at a later time.

It is always safe to visit the /Setup page and you will need to do so when updating to new versions of cloudscribe.Core whether via NuGet or synching from the source code repository.

In production installations that are up to date you maywish to disable the /Setup page, you can do that from config.json like this:

"SetupOptions": {
    "DisableSetup": "false"
  }
  
/Setup will return a 404 page not found response if disabled

