Contentful Excel Exporter
This project is a web application that allows users to export data from Contentful to an Excel file.
The application provides a form where users can select various options for the data they want to export, and then generates an Excel file based on these options.
Features
•	Export data from Contentful to an Excel file
•	Select specific locales and content types to export
•	Download the generated Excel file directly from the web application

Prerequisites
•	A Contentful account and access token

Things that need to be fixed:
export: 
• The reader's MaxDepth of 64 has been exceeded.
Querybuilder.Include(1):
to be able to fetch the linked values in SeoInfo, where I need SEO data and seoDescription.
But when I do, I get "The reader's MaxDepth of 64 has been exceeded" on a specific entry that has additionalContentDescription which has many links.
Now I have Querybuilder.Include(0) instead.

Import:
Must fix so that many values come in correctly. This function is not tested.

Remaining requirements:
• Make it possible to update all fields for regular and full DTO
• Make it possible to create a new value if there is no value.
• Create dropdown menus that fetch content types based on the Environment and space that the user has selected, for locales, and content types.
• It should be possible to log in with Ad (Active-Directory) credentials
• Handle successful/failed login
• Change so that it fetches from the content management API instead. To be able to see if an entry is archived.
• Create a generic DTO that adapts depending on which content type is to be exported. So that all fields are fetched automatically for each specific content type.
• Archived needs to run the management API and there should be an option if you want archived entries as well. Then it should be run with the management API instead.
• It should be possible to select locales during import. Now the app reads which locales are listed in each sheet.

