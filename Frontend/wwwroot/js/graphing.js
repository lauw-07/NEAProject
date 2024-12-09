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
        d3.select("#graph").selectAll("*").remove();

        const x = d3.scaleTime()
            .range([0, width]);

        const y = d3.scaleLinear()
            .range([height, 0]);

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

        x.domain(d3.extent(tempDataset, data => data.timestamp));
        y.domain([0, d3.max(tempDataset, data => data.value)]);

        const xAxisCall = d3.axisBottom(x)
            .ticks(d3.timeMonth.every(1))
            .tickFormat(d3.timeFormat("%b %Y"));

        svg.append("g")
            .attr("transform", `translate(0, ${height})`)
            .call(xAxisCall);

        const yAxisCall = d3.axisLeft(y);

        svg.append("g")
            .call(yAxisCall);
    }


    drawGraph();

    window.addEventListener("resize", drawGraph);
});
