let aws = require("aws-sdk"),
    config = require('./script_config');
const createCsvWriter = require('csv-writer').createObjectCsvWriter;
let format = require('date-format');
/*
*  Script to get file info from DynamoDB
*/
console.log("Script Started");

let region = config.region,
    profile = config.profile;
let dateStart = format.parse(format.ISO8601_FORMAT,config.createdStartDate);
 // Your timezone!
let creationStartDate = dateStart.getTime();
console.log(creationStartDate);
var dateEndDate = format.parse(format.ISO8601_FORMAT,config.createdEnddate);

 // Your timezone!
let creationEndDate = dateEndDate.getTime();
console.log(creationEndDate);
let date = new Date(),
    logTime = date.getHours() + "-" + date.getMinutes() + "-" + date.getSeconds();
const csvWriter = createCsvWriter({
  path: './logs/Logs' + date.toDateString() + "-" + logTime + '.csv',
  header: [
    {id: 'businessNo', title: 'Bus_No'},
    {id: 'publicId', title: 'CloudLocation'},
    {id: 'fileName', title: 'FileNameNoPath'},
    {id: 'privateId', title: 'PrivateId'},
	{id: 'creationDateFile', title: 'CreationDate'},
    {id: 'archievedDateFile', title: 'ArchivedDate'},
    {id: 'deletedAtFile', title: 'DeletedAt'},
	{id: 'stateFile', title: 'State'},
	{id: 'fileRestored', title: 'FileRestored'},
  ]
});
let credentials = new aws.SharedIniFileCredentials({ profile: profile });
aws.config.credentials = credentials;

let db = new aws.DynamoDB.DocumentClient({ region: region, credentials: credentials });
let lastEvaluatedKey = null,
    batch = 1,
    fileTableName = config.stackPrefix + "_Storage_File",
    metadataTableName = config.stackPrefix + "_Storage_FileMetadata";

executeScript();

//Function to get File info.
async function executeScript() {
	console.log("script execution started");
    let proceed = true;
    let data;
	let count = 0;
    while (proceed) {
        try {
            data = await getDBRecords(lastEvaluatedKey);
            lastEvaluatedKey = data.LastEvaluatedKey;
            if (data.Count > 0) {
                await getfileMetadata(data);
            }
        }
        catch (err) {
			console.log("error occured = " + JSON.stringify(err));
            proceed = false;
            return;
        }
        if (!data.LastEvaluatedKey || (data.Count === 0 && !data.LastEvaluatedKey)) {
            console.log("No records found");    
            proceed = false;
        }
    }
}

async function getDBRecords(lastEvaluatedKey) {
	console.log("db execution started");	
    return new Promise((resolve, reject) => {
        var params = {
            TableName: fileTableName,
            IndexName: "BusinessNo",
            KeyConditionExpression: "#a = :a",
            FilterExpression: "#s = :s AND #d BETWEEN :d1 AND :d2",
            ExpressionAttributeNames: {
                "#a": "BusinessNo",
				"#s": "State",
                 "#d": "CreationDate",
            },
            ExpressionAttributeValues: {
                ":a": Number(config.bu),
				":s": Number(config.state),
				":d1": creationStartDate,
                ":d2": creationEndDate
            },
            ExclusiveStartKey: lastEvaluatedKey
        };
        db.query(params, function (err, data) {
            if (err) {
                console.log("Error querying FileTable " + JSON.stringify(err));
                return reject(err);
            } else {
                return resolve(data);
            }
        });
    });
}




async function getfileMetadata(data) {
    const promises = data.Items.map(getMetadata);
    await Promise.all(promises);
    console.log("Batch " + batch + " Complete!");
    batch++;
}

async function getMetadata(item) {
    return new Promise(async (resolve, reject) => {
        var fileMetadataParams = {
            TableName: metadataTableName,
            KeyConditionExpression: "#Id = :Id AND #BusinessNo = :BusinessNo",
            ExpressionAttributeNames: {
                "#Id": "Id",
                "#BusinessNo": "BusinessNo",
            },
            ExpressionAttributeValues: {
                ":Id": item.Id,
                ":BusinessNo": item.BusinessNo
            }
        };
        db.query(fileMetadataParams, async function (err, result) {
            if (err) {
				console.log("Error in FileMetadata " + JSON.stringify(err));
                return reject(err);
            } else {
                if (result.Count > 0 && "Metadata" in result.Items[0]) {
                    await printLogs(result, item.PublicId,item.Id,item.CreationDate,item.ArchivedDate,item.DeletedAt,item.State);
                }
                return resolve();
            }
        });
    });
}

//Function to print logs to a log file
async function printLogs(result, publicId,id,creationDate,archivedDate,deletedAt,state) {
    return new Promise((resolve) => {
        let metadata = result.Items[0].Metadata;
		var creationDateNormalFormat = new Date(parseInt(creationDate));
		var archivedDateNormalFormat = new Date(parseInt(archivedDate));
		var deletedAtNormalFormat = new Date(parseInt(deletedAt));
		const data = [{businessNo:config.bu, publicId:publicId,fileName: result.Items[0].Metadata.FileName,privateId:id,
		creationDateFile:creationDateNormalFormat,archievedDateFile:archivedDateNormalFormat,deletedAtFile:deletedAtNormalFormat,stateFile:state}];
		csvWriter.writeRecords(data).then(()=> console.log(id));
        return resolve();
    });
}


