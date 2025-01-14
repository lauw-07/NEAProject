﻿using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class MainContainerComponent {

        [Parameter]
        public string? SelectedInstrument {  get; set; }

        [Parameter]
        public string? SelectedMarket { get; set; }

        private string? selectedStrategy;

        private void HandleStrategySelection(string strategy) {
            selectedStrategy = strategy;
        }

    }
}
