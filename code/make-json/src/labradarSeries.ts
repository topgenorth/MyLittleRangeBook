interface LabradarSeries {
    labradar: {
        deviceId: string,
        date: string,
        time: string,
        seriesName: string,
        totalNumberOfShots: number,
        units: {
            velocity: string,
            distance: string,
            weight: string
        },
        stats: {
            average: number,
            max: number,
            min: number,
            extremeSpread: number,
            standardDeviation: number
        },
        velocitiesInSeries: Array<number>
    },
    firearm : {
        name: string,
        cartridge: string
    },
    loadData: {
        cartridge: string,
        projectile: {
            name: string,
            weight: number,
            BC: {
                dragModel: string,
                value: number,
                sd: number
            }
        },
        powder: {
            name: string,
            amount: number
        }
    }

    tags: string[];
}