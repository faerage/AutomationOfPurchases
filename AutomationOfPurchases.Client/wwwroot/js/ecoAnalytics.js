// Зберігаємо всі інстанси, щоб потім їх знищити
const charts = {};

/* ---- спільний конструктор опцій ---- */
function buildOptions(yTitle, chartTitle) {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { position: 'top' },
            title: {
                display: true,
                text: chartTitle,
                align: 'center',
                font: { weight: 'bold', size: 16 }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                title: { display: true, text: yTitle }
            }
        }
    };
}

/* ---- головний експортований метод ---- */
export function drawCharts(itemId,
    labels,
    approved,
    rejected,
    warehouseQty,
    purchaseQty,
    unit) {

    disposeCharts(itemId);             // якщо графіки вже існували
    charts[itemId] = [];

    /* ---- статус заявок (затверджено / відхилено) ---- */
    charts[itemId].push(
        new Chart(
            document.getElementById(`statusChart_${itemId}`),
            {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        { label: 'Затверджено (шт)', data: approved, fill: false },
                        { label: 'Відхилено (шт)', data: rejected, fill: false }
                    ]
                },
                options: buildOptions('Кількість, шт', 'Статус заявок')
            }));

    /* ---- рішення (кількість, з одиницею зберігання) ---- */
    charts[itemId].push(
        new Chart(
            document.getElementById(`decChart_${itemId}`),
            {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        { label: `Доставити зі складу (${unit})`, data: warehouseQty, fill: false },
                        { label: `Закупити (${unit})`, data: purchaseQty, fill: false }
                    ]
                },
                options: buildOptions(`Кількість, ${unit}`, 'Рішення')
            }));

}

/* ---- очищення при згортанні карти ---- */
export function disposeCharts(itemId) {
    if (!charts[itemId]) return;
    charts[itemId].forEach(c => c.destroy());
    delete charts[itemId];
}
