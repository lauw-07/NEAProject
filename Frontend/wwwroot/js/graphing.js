const margin = { top: 70, right: 70, bottom: 70, left: 70 }

function GetDimensions(divId) {
    /*const parentDiv = document.getElementById("graph");
    const width = parentDiv.clientWidth - margin.top - margin.bottom;
    const height = parentDiv.clientHeight - margin.left - margin.right;

    dimensions[0] = width;
    dimensions[1] = height;*/

    const width = parseInt(d3.select(`#${divId}`).style('width')) - margin.top - margin.bottom;
    const height = parseInt(d3.select(`#${divId}`).style('height')) - margin.left - margin.right;

    return { width, height }
};

function CreateTooltip() {
    return d3.select("body").append("div")
        .attr("class", "tooltip");
}

function UpdateTooltip(tooltip, mouseEvent, content) {
    tooltip.style("display", "block")
        .html(content)
        .style("left", (mouseEvent.pageX + 10) + "px")
        .style("top", (mouseEvent.pageY - 10) + "px");

    // mouseEvent is to keep track of where the mouse is and so let's the function know where to create the tooltip relative to the mouse
}

function HideTooltip(tooltip) {
    tooltip.style("display", "none");
}

function DrawGraph(datasets, divId) {
    const { width, height } = GetDimensions(divId);

    if (!Array.isArray(datasets)) {
        datasets = [datasets];  
    }
    
    //To create new instances of the svg component so that it can be updated with new dimensions
    d3.select(`#${divId}`).selectAll("*").remove();
    
    const svg = d3.select(`#${divId}`)
        .append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);

    let pnlData = null;
    let priceData = {};

    Object.entries(datasets).forEach(([label, data]) => {
        if (label === "pnl") {
            pnlData = data;
        } else {
            priceData[label] = data;
        }
    });

    const timestamps = Object.values(datasets).flatMap(data => data.timestamps);
    const priceDataValues = Object.values(priceData).flatMap(data => data.timestamps);
    const pnlValues = pnlData.values;
    const padding = 20;



    const x = d3.scaleTime()
        .domain(d3.extent(timestamps, timestamp => d3.timeParse("%d/%m/%Y")(timestamp)))
        .range([padding, width]);

    const xAxisCall = d3.axisBottom(x)
        .ticks(d3.timeMonth.every(3))
        .tickFormat(d3.timeFormat("%b %Y"));

    svg.append("g")
        .attr("transform", `translate(0, ${height})`)
        .style("font-size", "13px")
        .call(xAxisCall)
        .call(g => g.select(".domain").remove())
        .selectAll(".tick line")
        .style("stroke-opacity", 0)

    svg.selectAll(".tick text")
        .attr("fill", "#a0a3a1")

    const yPriceData = d3.scaleLinear()
        .domain(d3.extent(priceDataValues))
        .range([height, 0]);    

    const yAxisCall = d3.axisLeft(yPriceData);
    svg.append("g")
        .style("font-size", "13px")
        .call(yAxisCall)
        .call(g => g.select(".domain").remove())
        .style("stroke-opacity", 0)
        .selectAll(".tick text")
        .style("fill", "#a0a3a1")

    if (pnlData) {
        const yPnlData = d3.scaleLinear()
            .domain(d3.extent(pnlValues))
            .range([height, 0]);

        const yPnlAxisCall = d3.axisRight(yPnlData);
        svg.append("g")
            .style("font-size", "13px")
            .call(yPnlAxisCall)
            .call(g => g.select(".domain").remove())
            .style("stroke-opacity", 0)
            .selectAll(".tick text")
            .style("fill", "#a0a3a1")

        const formattedPnlDataset = yPnlData.timestamps.map((timestamp, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(timestamp),
            value: yPnlData.values[index]
        }));

        svg.append("path")
            .datum(formattedPnlDataset)
            .attr("fill", "none")
            .attr("stroke", "#ff7f0e")
            .attr("stroke-width", 1.5)
            .attr("stroke-dasharray", "4,4")
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => yPnlData(datapoint.value))
            );
    }

    svg.selectAll("verticalGrid")
        .data(x.ticks())
        .join("line")
        .attr("x1", d => x(d))
        .attr("x2", d => x(d))
        .attr("y1", 0)
        .attr("y2", height)
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)

    svg.selectAll("horizontalGrid")
        .data(y.ticks())
        .join("line")
        .attr("x1", 0)
        .attr("x2", width + padding)
        .attr("y1", d => y(d))
        .attr("y2", d => y(d))
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 0.25)


    const tooltip = CreateTooltip();

    const crosshair = svg.append("g")
        .style("display", "none")

    crosshair.append("line")
        .attr("id", "crosshairXdir")
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 1.5)
        .attr("stroke-opacity", 2)
        .attr("stroke-dasharray", "2,2")

    crosshair.append("line")
        .attr("id", "crosshairYdir")
        .attr("stroke", "#a0a3a1")
        .attr("stroke-width", 1.5)
        .attr("stroke-opacity", 2)
        .attr("stroke-dasharray", "2,2")

    svg.append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("fill", "none")
        .attr("pointer-events", "all")
        .on("mouseover", event => {
            crosshair.style("display", null);
            UpdateCrosshair(event, tooltip)
        })
        .on("mousemove", event => UpdateCrosshair(event, tooltip))
        .on("mouseout", () => {
            crosshair.style("display", null);
        });

    function UpdateCrosshair(event, tooltip) {
        const [mouseXcoord, mouseYcoord] = d3.pointer(event, svg.node());

        d3.select("#crosshairXdir")
            .attr("x1", mouseXcoord)
            .attr("x2", mouseXcoord)
            .attr("y1", 0)
            .attr("y2", height);

        d3.select("#crosshairYdir")
            .attr("x1", 0)
            .attr("x2", width)
            .attr("y1", mouseYcoord)
            .attr("y2", mouseYcoord);

        UpdateTooltip(tooltip, event, "");
    }

    //need to figure out how to draw multiple line graphs on the same chart
    //A TS object contains List of timestamps and a List of values
    const color = d3.scaleOrdinal(d3.schemeCategory10);
    let i = 0;
    Object.entries(priceData).forEach(([label, data]) => {
        const formattedDataset = data.timestamps.map((timestamp, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(timestamp),
            value: data.values[index]
        }));

        svg.append("path")
            .datum(formattedDataset)
            .attr("fill", "none")
            .attr("stroke", color(i))
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => yPriceData(datapoint.value))
            );
        i++;
    });
};