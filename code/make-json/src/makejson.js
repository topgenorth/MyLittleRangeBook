"use strict";
const fs = require('fs');
const rd = require('readline');
// fs.readdir(testFolder, (err, files) => {
//     files.forEach(file => {
//         console.log(file);
//     });
// });
// const testFile = '/Users/tomo/work/labradar/LBR/SR0042/SR0042 Report.csv';
const blogDataDir = '/Users/tomo/work/topgenorth.github.io/data/labradar/';
const labradarFolder = '/Users/tomo/work/labradar/LBR/';
let labradarSeries = {
    labradar: {
        deviceId: ``,
        date: ``,
        time: ``,
        seriesName: ``,
        totalNumberOfShots: 0,
        units: {
            velocity: ``,
            distance: ``,
            weight: ``
        },
        stats: {
            average: 0,
            max: 0,
            min: 0,
            extremeSpread: 0,
            standardDeviation: 0
        },
        velocitiesInSeries: Array()
    },
    firearm: {
        name: ``,
        cartridge: ``
    },
    loadData: {
        cartridge: ``,
        projectile: {
            name: ``,
            weight: 0,
            ballisticCoefficient: {
                dragModel: ``,
                value: 0,
                sd: 0
            }
        },
        powder: {
            name: ``,
            amount: 0
        }
    },
    notes: ``,
    tags: Array()
};
let count = 0;
fs.readdirSync(labradarFolder).forEach((entry) => {
    if (entry.startsWith('SR')) {
        const root = entry.split(".")[0];
        const sourceFile = labradarFolder + root + "/" + root + " Report.csv";
        if (fs.existsSync(sourceFile)) {
            convertLabradarFileForBlog(sourceFile);
        }
        else {
            console.log("'" + sourceFile + "' does not exist.");
        }
    }
});
function convertLabradarFileForBlog(filename) {
    count = 0;
    labradarSeries = {
        labradar: {
            deviceId: ``,
            date: ``,
            time: ``,
            seriesName: ``,
            totalNumberOfShots: 0,
            units: {
                velocity: ``,
                distance: ``,
                weight: ``
            },
            stats: {
                average: 0,
                max: 0,
                min: 0,
                extremeSpread: 0,
                standardDeviation: 0
            },
            velocitiesInSeries: Array()
        },
        firearm: {
            name: ``,
            cartridge: ``
        },
        loadData: {
            cartridge: ``,
            projectile: {
                name: ``,
                weight: 0,
                ballisticCoefficient: {
                    dragModel: ``,
                    value: 0,
                    sd: 0
                }
            },
            powder: {
                name: ``,
                amount: 0
            }
        },
        notes: ``,
        tags: Array()
    };
    console.log("Processing file '" + filename + "'.");
    const reader = rd.createInterface(fs.createReadStream(filename));
    reader.on("line", parseLineFromFile);
    reader.on("close", calculateInformationForSeries);
}
function parseLineFromFile(l) {
    switch (count) {
        case 0:
            // separator
            break;
        case 1:
            labradarSeries.labradar.deviceId = getValue(l);
            break;
        case 3:
            // Series name
            labradarSeries.labradar.seriesName = `SR` + getValue(l);
            break;
        case 6:
            // units - velocity
            labradarSeries.labradar.units.velocity = getValue(l);
            break;
        case 7:
            // units - distance
            labradarSeries.labradar.units.distance = getValue(l);
            break;
        case 9:
            // units - weight
            labradarSeries.labradar.units.weight = getValue(l);
            break;
        default:
            if (count > 17) {
                const velocity = Number(getValue(l));
                const idx = labradarSeries.labradar.velocitiesInSeries.length;
                labradarSeries.labradar.velocitiesInSeries.push(velocity);
            }
            if (count === 18) {
                let cells = l.split(';');
                let dateIdx = cells.length - 3;
                labradarSeries.labradar.time = cells[dateIdx + 1];
                labradarSeries.labradar.date = cells[dateIdx];
            }
            break;
    }
    count++;
}
function saveJsonForWebsite(labradarSeries) {
    let destination = blogDataDir + labradarSeries.labradar.seriesName + '.json';
    let jsonData = JSON.stringify(labradarSeries);
    // @ts-ignore
    fs.writeFile(destination, jsonData, function (err) {
        if (err) {
            console.log(err);
        }
    });
    console.log("  Wrote JSON to '" + destination + "'.");
}
function calculateInformationForSeries() {
    const v0 = labradarSeries.labradar.velocitiesInSeries.slice().sort();
    labradarSeries.labradar.totalNumberOfShots = v0.length;
    labradarSeries.labradar.stats.max = v0[v0.length - 1];
    labradarSeries.labradar.stats.min = v0[0];
    labradarSeries.labradar.stats.extremeSpread = labradarSeries.labradar.stats.max - labradarSeries.labradar.stats.min;
    labradarSeries.labradar.stats.average = average(v0);
    labradarSeries.labradar.stats.standardDeviation = Math.round(standardDeviation(v0) * 10) / 10;
    saveJsonForWebsite(labradarSeries);
}
function average(someValues) {
    const avg = someValues.reduce((a, b) => (a + b)) / someValues.length;
    return Math.round(avg);
}
function standardDeviation(someValues) {
    const avg = average(someValues);
    const squareDiffs = someValues.map(function (value) {
        const diff = value - avg;
        return diff * diff;
    });
    const avgSquareDiff = average(squareDiffs);
    return Math.sqrt(avgSquareDiff);
}
function getValue(l) {
    let parts = l.split(';');
    return parts[1];
}
