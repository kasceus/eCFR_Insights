window.agencyTreemap = {
    render: function (data) {

        const container = document.getElementById("treemapWrapper");

        const width = container.clientWidth || window.innerWidth;
        const height = container.clientHeight || window.innerHeight;


        d3.select("#agencyTreemap").selectAll("*").remove();

        const svg = d3.select("#agencyTreemap")
            .attr("width", width)
            .attr("height", height)
            .attr("viewBox", [0, 0, width, height])
            .style("font-family", "sans-serif");


        const root = d3.treemap()
            .size([width, height])
            .padding(2)
            (d3.hierarchy(data)
                .sum(d => d.value)
                .sort((a, b) => b.value - a.value));

        const color = d3.scaleSequential([0, root.height], d3.interpolateBlues);

        const tooltip = d3.select("body")
            .append("div")
            .style("position", "absolute")
            .style("padding", "8px")
            .style("background", "rgba(0,0,0,0.75)")
            .style("color", "white")
            .style("border-radius", "4px")
            .style("pointer-events", "none")
            .style("opacity", 0);

        const nodes = svg.selectAll("g")
            .data(root.leaves())
            .join("g")
            .attr("transform", d => `translate(${d.x0},${d.y0})`)
            .style("cursor", "pointer")
            .on("click", (event, d) => {
                window.location.href = `/agency/${d.data.slug}`;
            })
            .on("mouseover", (event, d) => {
                tooltip.transition().duration(150).style("opacity", 1);
                tooltip.html(`
                    <strong>${d.data.name}</strong><br/>
                    Hierarchies: ${d.data.totalHierarchies}<br/>
                    Words: ${d.data.totalWords}<br/>
                    Sub‑Agencies: ${d.data.totalSubAgencies}<br/>
                    Complexity: ${d.data.value.toFixed(3)}
                `);
            })
            .on("mousemove", (event) => {
                tooltip.style("left", (event.pageX + 10) + "px")
                       .style("top", (event.pageY + 10) + "px");
            })
            .on("mouseout", () => {
                tooltip.transition().duration(150).style("opacity", 0);
            });

        nodes.append("rect")
            .attr("width", d => d.x1 - d.x0)
            .attr("height", d => d.y1 - d.y0)
            .attr("fill", d => color(d.depth));

        nodes.append("text")
            .attr("x", 4)
            .attr("y", 14)
            .text(d => d.data.name)
            .style("font-size", "12px")
            .style("fill", "white")
            .style("pointer-events", "none")
            .style("opacity", d => (d.x1 - d.x0) > 60 && (d.y1 - d.y0) > 20 ? 1 : 0);
    }
};
