import { CreateXScale, CreateYScale } from "./generateScales";
import SetUpGraph from "./graphSetup";
import { CreateTooltip, HideTooltip } from "./tooltip"; 
import { CreateCrosshair, UpdateCrosshair } from "./crosshair";

const margin = { top: 70, right: 70, bottom: 70, left: 70 }

function GetDimensions(divId) {
    const width = parseInt(d3.select(`#${divId}`).style('width')) - margin.top - margin.bottom;
    const height = parseInt(d3.select(`#${divId}`).style('height')) - margin.left - margin.right;
    return { width, height }
};

export default function DrawGraph(datasets, divId) {
    const { width, height } = GetDimensions(divId);

    //To create new instances of the svg component so that it can be updated with new dimensions
    d3.select(`#${divId}`).selectAll("*").remove();

    const svg = d3.select(`#${divId}`)
        .append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);

    const pnlData = datasets["pnl"] ? datasets["pnl"] : null;
    let priceData = {};

    Object.entries(datasets).forEach(([label, data]) => {
        if (label !== "pnl") {
            priceData[label] = data;
        } 
    });

    const timestamps = Object.values(datasets).flatMap(data => data[0]["timestamps"]);
    const formattedTimestamps = timestamps.map(ts => d3.timeParse("%d/%m/%Y")(ts));
    const padding = 20;
    const x = CreateXScale(formattedTimestamps, width);

    const priceDataValues = Object.values(priceData).flatMap(data => data[1]["values"]);
    const yPriceData = CreateYScale(priceDataValues, height);

    let y = [yPriceData];
    let pos = ["left"];

    if (pnlData) {
        const yPnl = CreateYScale(pnlData.values, height);
        y.push(yPnl);
        pos.push("right");

        const formattedPnlData = pnlData[0]["timestamps"].map((timestamp, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(timestamp),
            value: pnlData[1]["values"][index]
        }));

        svg.append("path")
            .datum(formattedPnlData)
            .attr("fill", "none")
            .attr("stroke", "red")
            .attr("stroke-width", 1.5)
            .attr("d", d3.line()
                .x(datapoint => x(datapoint.timestamp))
                .y(datapoint => yPnl(datapoint.value))
            );
    }

    SetUpGraph(svg, x, y, height, width, pos, (pnlData) ? true : false);

    const tooltip = CreateTooltip();
    const crosshair = CreateCrosshair(svg);

    svg.append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("fill", "none")
        .attr("pointer-events", "all")
        .on("mouseover", event => {
            crosshair.style("display", "block");
            UpdateCrosshair(event, tooltip, svg, height, width, x, datasets);
        })
        .on("mousemove", event => UpdateCrosshair(event, tooltip, svg, height, width, x, datasets))
        .on("mouseout", () => {
            crosshair.style("display", "none");
        });

    const color = d3.scaleOrdinal(d3.schemeCategory10);

    Object.entries(priceData).forEach(([label, data], i) => {
        const formattedDataset = data[0]["timestamps"].map((timestamp, index) => ({
            timestamp: d3.timeParse("%d/%m/%Y")(timestamp),
            value: data[1]["values"][index]
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