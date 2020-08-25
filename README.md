# har2zip
Converts a HAR file to a Zip file. 

This only converts the contents of the requests into a combined zip file, and will not contain metadata like cookies and http headers.

The HTTP Archive format, or HAR, is a JSON-formatted archive file format for logging of a web browser's interaction with a site. The common extension for these files is .har.

The specification for the HTTP Archive (HAR) format defines an archival format for HTTP transactions that can be used by a web browser to export detailed performance data about web pages it loads. The specification for this format is produced by the Web Performance Working Group of the World Wide Web Consortium (W3C). The specification is in draft form and is a work in progress.

You can record your HTTP session using the Network tab in the Developer Tools in Chrome.

* Open the Developer Tools from the menu (Menu > More Tools > Developer tools), or by pressing Ctrl+Shift+C on your keyboard
* Click on the Network tab
* Look for a round button at the top left of the Network tab. Make sure it is red. If it is grey, click it once to start recording.
* Check the box next to Preserve log
* Save the capture by right-clicking on the grid and choosing "Save as HAR with Content"

# To build

    dotnet publish

# Usage

    har2zip.exe filename.har

Outputs

    filename.har.zip
