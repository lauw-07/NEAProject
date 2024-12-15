const margin = { top: 70, right: 70, bottom: 70, left: 70 }

let parentDiv = document.getElementById("graph");
function GetDimensions() {
    let height = parentDiv.clientHeight - margin.left - margin.right;
    let width = parentDiv.clientWidth - margin.top - margin.bottom;
    return { width, height };
    };

function DrawGraph(dataset) {
    const { width, height } = GetDimensions();

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


    //need to figure out how to draw multiple line graphs on the same chart

    svg.append("path")
        .datum(dataset)
        .attr("fill", none)
        .attr("stroke", "steelblue")
        .attr("stroke-width", 1.5)
        .attr("d", d3.line()
            .x(d => x(d.timestamp))
            .y(d => y(d.value)))
}

//window.addEventListener("resize", DrawGraph);