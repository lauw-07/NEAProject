document.addEventListener("DOMContentLoaded", () => {
    const margin = { top: 70, right: 70, bottom: 70, left: 70 }

    let parentDiv = document.getElementById("graph");
    function getDimensions() {
        let height = parentDiv.clientHeight - margin.left - margin.right;
        let width = parentDiv.clientWidth - margin.top - margin.bottom;
        return { width, height };
    };

    function drawGraph() {
        const { width, height } = getDimensions();

        d3.select("#graph").selectAll("*").remove(); //To create new instances of the svg component so that it can be updated with new dimensions

        const x = d3.scaleTime()
            .domain(d3.extent(tempDataset, data => data.timestamp))
            .range([0, width]);

        const xAxisCall = d3.axisBottom(x)
            .ticks(d3.timeMonth.every(1))
            .tickFormat(d3.timeFormat("%b %Y"));
        svg.append("g")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxisCall);

        const y = d3.scaleLinear()
            .domain([0, d3.max(tempDataset, data => data.value)])
            .range([height, 0]);

        const yAxisCall = d3.axisLeft(y);
        svg.append("g")
            .call(yAxisCall);

        const svg = d3.select("#graph")
            .append("svg")
            .attr("width", width + margin.left + margin.right)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);


        //Perhaps use a json file or similar to store the Timeseries with the relevant timestamps and values
        //I can keep adding to the file when loading data in from the api


        const tempDataset = [
            { timestamp: new Date("2022-01-01"), value: 200 },
            { timestamp: new Date("2022-02-01"), value: 250 },
            { timestamp: new Date("2022-03-01"), value: 180 },
            { timestamp: new Date("2022-04-01"), value: 300 },
            { timestamp: new Date("2022-05-01"), value: 280 },
            { timestamp: new Date("2022-06-01"), value: 220 },
            { timestamp: new Date("2022-07-01"), value: 300 },
            { timestamp: new Date("2022-08-01"), value: 450 },
            { timestamp: new Date("2022-09-01"), value: 280 },
            { timestamp: new Date("2022-10-01"), value: 600 },
            { timestamp: new Date("2022-11-01"), value: 780 },
            { timestamp: new Date("2022-12-01"), value: 320 }
        ];

        /*
        Code to use real-life data, implement once figured out how to store the data
        
        const dataset = timeseriesData.map(dataPair => ({
            timestamp: d3.timeParse("%Y-%m-%d")(dataPair.timestamp),
            value: dataPair.value
        }));

        */
        
        svg.append("path")
            .datum(tempDataset)
            .attr("fill", none)
            .attr("stroke", "steelblue")
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(d => x(d.timestamp))
                .y(d => y(d.value)))
    }

    
    drawGraph();

    window.addEventListener("resize", drawGraph);
});
