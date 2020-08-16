# Crittr's Unity SDK

This package contains the SDK for Crittr's API in Unity.

### Installation
*Currently in Beta and not available on the asset store. Will publish when a major
release is ready*

#### Install the SDK

#### Install from Github

1. Use the Github url for the master branch, or the latest release found in the releases tab. 

2. Follow the instructions from the [official Unity documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html) to install the SDK.

#### Install locally

1. Clone or download and extract the repository to a local folder (e.g. `C:\Downloads\Crittr\unity-sdk`).

2. Open up Unity using an exising project or create a new one.
    * In Unity, open the package manager (`Window > Package Manager`)
    * From the package manager, click the `+` icon on the top left and select the option `Add package from disk...`
    * Navigate to the downloaded repository and select the `package.json` file. (e.g. `C:\Downloads\Crittr\unity-sdk\package.json`)
    * Open the `package.json` file and the SDK should install.

### Using the SDK

#### Setting up in Unity

CrittrReporter and CrittrCanvas prefabs exist in the `Assets/Prefabs` directory to help send your first report. CrittrReporter sends the reports, whereas CrittrCanvas displays the manual reports UI.

#### Sending Automatic Reports

1. Add the CrittrReporter prefab to your Scene.

2. Add the Connection URI to the CrittrReporter prefab:
    * Scroll to the CrittrSDK script.
    * Input the Connection URI for your project (you can find this in the project settings SDK section on [Crittr's dashboard](https://dashboard.crittr.co))

3. Check the `Send Automatic Reports` in the CrittrSDK script Options.

4. Run your game and throw an exception.
    * If you get a log message with a location to your report on the dashboard, your report has been sent successfully. Go to the Crittr dashboard to see the report.
    * If you got a log error, check that you have entered a valid connection uri.


#### Sending Manual Reports

1. Add the CrittrReporter and CrittrCanvas prefabs to your Scene as game objects.

2. Add the Connection URI to the CrittrReporter game object:
    * Scroll to the CrittrSDK script.
    * Input the Connection URI for your project (you can find this in the project settings SDK section on [Crittr's dashboard](https://dashboard.crittr.co))

3. In the CrittrReporter game objects's CrittrSDK script, add the CrittrCanvas game object from your Scene to the following report lifecycle events:
    * On Show Form, select the `CrittrUIManager -> HandleShowForm` function.
    * On Report Success, select the `CrittrUIManager -> HandleShowSuccess` function.
    * On Report Failure, select the `CrittrUIManager -> HandleShowFailure` function.

4. In the CrittrCanvas game object, scroll to the Crittr UI Manager script. Add the CrittrReporter game object to the Crittr Reporter selection. 

5. Run your game and then press `F8`. A screenshot of your game will be taken and a success screen should pop up with a link and QR code to update your report.
    * If a screen does not pop up, check the logs to see if you have inputted the connection uri correctly.
    * If the failure screen shows, check that you have a valid connection uri.

6. Navigate to the report using the QR code or link, then edit it with a new title and description.

7. That's it! You should be able to see a screenshot and the report in the reports list of the project.
