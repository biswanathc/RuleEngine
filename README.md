This has been developed as an webapi. This solution is to be built, published and hosted in IIS. the url will be http://<server>:<port>/api/values.
This api will return a list of failed signal as JSON.
Ruleset and json has been kept under App Data folder. API will get payload and ruleset from files. In case of any change in rule please edit the file content without making change in filename and location and structure.
In ruleset when value type is 0 then it means it checks for LOW signal. When it is 1 then it checks for high signal.
For value type datetime, by default it is null. This means it will compare against current date. If a valid datetime is specified then check will happen against that date.
