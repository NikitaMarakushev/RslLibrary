window.charts = {};

window.createChart = (id, type, labels, data, colors, title) => {
    const ctx = document.getElementById(id);

    if (!ctx) {
        console.warn(`Canvas with id '${id}' not found`);
        return;
    }

    if (window.charts[id]) {
        window.charts[id].destroy();
    }

    const backgroundColor = colors && colors.length > 0
        ? colors
        : 'rgba(54, 162, 235, 0.5)';
    const borderColor = colors && colors.length > 0
        ? colors.map(c => c.replace('0.5', '1'))
        : 'rgba(54, 162, 235, 1)';

    window.charts[id] = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: title,
                data: data,
                backgroundColor: backgroundColor,
                borderColor: borderColor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: type === 'pie' || type === 'doughnut',
                    position: 'bottom'
                },
                title: {
                    display: true,
                    text: title
                }
            },
            scales: type === 'bar' || type === 'line' ? {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0
                    }
                }
            } : {}
        }
    });
};

window.destroyChart = (id) => {
    if (window.charts[id]) {
        window.charts[id].destroy();
        delete window.charts[id];
    }
};