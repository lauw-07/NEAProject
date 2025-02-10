import { UpdateTooltip } from "./tooltip.js";

export function CreateCrosshair(svg) {
    const crosshair = svg.append("g")
        .style("display", "none");

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

    return crosshair;
}

export function UpdateCrosshair(event, tooltip, svg, height, width, xScale, dataset) {
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

    //Ts = timestamp
    const mouseTs = xScale.invert(mouseXcoord);
    let closestValues = {};

    Object.entries(dataset).forEach(([label, data]) => {
        const timestamps = data.flatMap(d => d[0]["timestamps"]);
        const values = data.flatMap(d => d[1]["values"]);

        const closestIndex = timestamps.reduce((prev, curr, index) => {
            if (Math.abs(curr - mouseTs) < Math.abs(timestamps[prev] - mouseTs)) {
                return index;
            } else {
                return prev;
            }
        }, 0);

        const closestValue = values[closestIndex];
    });

    let toolTipContent = "";

    Object.entries(closestValues).forEach(([label, value]) => {
        toolTipContent += `${label}: ${value}`;
    });

    UpdateTooltip(tooltip, event, toolTipContent);
}