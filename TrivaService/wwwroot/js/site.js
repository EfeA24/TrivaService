(function () {
    function getListFilterSettings() {
        var defaults = { enabled: true, autoApply: false };
        var runtime = window.TrivaSettings && window.TrivaSettings.listFilter
            ? window.TrivaSettings.listFilter
            : {};
        return {
            enabled: runtime.enabled !== false,
            autoApply: runtime.autoApply === true
        };
    }

    function debounce(fn, delay) {
        var timer;
        return function () {
            var args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () {
                fn.apply(null, args);
            }, delay);
        };
    }

    function extractSearchTermFromFilter(rawFilter) {
        if (!rawFilter) return "";
        var match = rawFilter.match(/'([^']+)'/);
        return match && match[1] ? decodeURIComponent(match[1]).replace(/''/g, "'") : "";
    }

    function extractActiveStateFromFilter(rawFilter) {
        if (!rawFilter) return "";
        var normalized = rawFilter.toLowerCase();
        if (normalized.indexOf("isactive eq true") >= 0) return "true";
        if (normalized.indexOf("isactive eq false") >= 0) return "false";
        return "";
    }

    function buildODataFilter(path, term) {
        var normalizedPath = (path || "").toLowerCase();
        var escaped = (term || "").toLowerCase().replace(/'/g, "''");
        var map = {
            "/customers": ["CustomerName", "CustomerPhone", "CustomerAddress"],
            "/services": ["ServiceCode", "Status", "ServiceAddress", "FaultDescription"],
            "/items": ["ItemName", "ItemCode", "ItemBrand", "ItemModel"],
            "/suppliers": ["SupplierName", "SupplierContactPerson", "SupplierPhone"],
            "/serviceitems": ["Notes"],
            "/servicevisuals": ["ServiceVisualName", "Notes", "ServiceDocumentUrl"],
            "/roles": ["RoleName", "RoleDescription"],
            "/users": ["UserName", "UserPhone", "UserNotes"]
        };

        var fields = map[normalizedPath];
        if (!fields || !fields.length || !escaped) return "";

        return fields.map(function (field) {
            return "contains(tolower(" + field + "),'" + escaped + "')";
        }).join(" or ");
    }

    function initTopbarSearch() {
        var input = document.getElementById("globalPageSearch");
        if (!input) return;

        var menuLinks = Array.prototype.slice.call(document.querySelectorAll(".menu-link[data-controller]"));
        var resolver = debounce(function (query) {
            var q = (query || "").trim().toLowerCase();
            if (!q) return;

            var match = menuLinks.find(function (link) {
                var label = (link.textContent || "").trim().toLowerCase();
                return label.indexOf(q) !== -1;
            });

            if (match) {
                window.location.href = match.getAttribute("href");
            }
        }, 300);

        input.addEventListener("keydown", function (event) {
            if (event.key !== "Enter") return;
            event.preventDefault();
            resolver(input.value);
        });
    }

    function initListSearch() {
        var settings = getListFilterSettings();
        if (!settings.enabled) return;

        var table = document.querySelector(".card .table");
        var card = document.querySelector(".card.shadow-sm");
        if (!table || !card) return;
        if (document.querySelector(".list-search-bar")) return;

        var path = window.location.pathname.toLowerCase();
        if (!/^\/(customers|services|items|suppliers|serviceitems|servicevisuals|roles|users)(\/index)?$/.test(path)) {
            return;
        }

        var queryParams = new URLSearchParams(window.location.search);
        var currentFilter = queryParams.get("$filter") || "";
        var currentSearch = extractSearchTermFromFilter(currentFilter);
        var activeFilter = extractActiveStateFromFilter(currentFilter);

        var wrapper = document.createElement("div");
        wrapper.className = "list-search-bar";
        wrapper.innerHTML =
            '<div class="list-search-controls">' +
                '<div class="input-group list-search-group">' +
                    '<span class="input-group-text">Ara</span>' +
                    '<input type="search" class="form-control" id="listFilterInput" placeholder="Liste içinde ara..." value="' + currentSearch + '">' +
                    '<button type="button" class="btn btn-primary list-search-btn ms-2" id="listFilterButton">Ara</button>' +
                '</div>' +
                '<select id="listActiveFilter" class="form-select list-active-filter">' +
                    '<option value="">Durum: Tümü</option>' +
                    '<option value="true">Durum: Aktif</option>' +
                    '<option value="false">Durum: Pasif</option>' +
                '</select>' +
            '</div>';
        card.insertBefore(wrapper, card.firstChild);

        var input = wrapper.querySelector("#listFilterInput");
        var searchBtn = wrapper.querySelector("#listFilterButton");
        var activeSelect = wrapper.querySelector("#listActiveFilter");
        if (activeSelect && activeFilter !== null) {
            activeSelect.value = activeFilter;
        }

        function applyFilter(term) {
            var params = new URLSearchParams(window.location.search);
            var selectedActive = activeSelect ? activeSelect.value : "";
            var searchFilter = term && term.trim()
                ? buildODataFilter(path.replace("/index", ""), term)
                : "";
            var clauses = [];
            if (searchFilter) clauses.push("(" + searchFilter + ")");
            if (selectedActive === "true" || selectedActive === "false") {
                clauses.push("IsActive eq " + selectedActive);
            }
            if (clauses.length) params.set("$filter", clauses.join(" and "));
            else params.delete("$filter");

            var next = window.location.pathname + (params.toString() ? ("?" + params.toString()) : "");
            window.location.href = next;
        }

        var applyFilterDebounced = debounce(function (term) {
            applyFilter(term);
        }, 400);

        if (settings.autoApply) {
            input.addEventListener("input", function () {
                applyFilterDebounced(input.value);
            });
        }

        if (searchBtn) {
            searchBtn.addEventListener("click", function () {
                applyFilter(input.value);
            });
        }

        input.addEventListener("keydown", function (event) {
            if (event.key !== "Enter") return;
            event.preventDefault();
            applyFilter(input.value);
        });

        if (activeSelect) {
            activeSelect.addEventListener("change", function () {
                applyFilter(input.value);
            });
        }
    }

    function initRemoteSelect($element, config) {
        if (!$element || !$element.length || !$element.select2) return;
        if ($element.hasClass("select2-hidden-accessible")) return;

        var selectedId = $element.val();
        $element.select2({
            width: "100%",
            allowClear: true,
            placeholder: config.placeholder,
            dropdownParent: $element.closest(".modal").length ? $element.closest(".modal") : $(document.body),
            ajax: {
                url: config.lookupUrl,
                dataType: "json",
                delay: 250,
                data: function (params) {
                    return {
                        term: params.term || "",
                        page: params.page || 1
                    };
                },
                processResults: function (data) {
                    var items = data && data.value ? data.value : [];
                    return {
                        results: items,
                        pagination: { more: false }
                    };
                }
            }
        });

        if (selectedId && config.byIdUrl) {
            $.getJSON(config.byIdUrl.replace("__id__", selectedId), function (item) {
                if (!item || !item.id) return;
                var option = new Option(item.text, item.id, true, true);
                $element.append(option).trigger("change");
            });
        }
    }

    function initForeignKeySelects(container) {
        var root = container || document;
        var forms = root.querySelectorAll("form");
        if (!forms.length) return;

        var mapping = {
            CustomerId: { lookupUrl: "/odata/customers/lookup", byIdUrl: "/odata/customers/lookup/__id__", placeholder: "Müşteri seçin" },
            SupplierId: { lookupUrl: "/odata/suppliers/lookup", byIdUrl: "/odata/suppliers/lookup/__id__", placeholder: "Tedarikçi seçin" },
            ServiceId: { lookupUrl: "/odata/services/lookup", byIdUrl: "/odata/services/lookup/__id__", placeholder: "Servis seçin" },
            ItemId: { lookupUrl: "/odata/items/lookup", byIdUrl: "/odata/items/lookup/__id__", placeholder: "Ürün seçin" },
            RolesId: { lookupUrl: "/odata/roles/lookup", byIdUrl: "/odata/roles/lookup/__id__", placeholder: "Rol seçin" }
        };

        Object.keys(mapping).forEach(function (name) {
            var input = root.querySelector('input[name="' + name + '"]');
            if (!input || input.dataset.select2Ready === "1") return;

            var select = document.createElement("select");
            select.name = name;
            select.id = input.id || name;
            select.className = "form-select fk-select";
            select.dataset.select2Ready = "1";
            select.value = input.value || "";
            if (input.required) select.required = true;
            if (input.getAttribute("data-val")) select.setAttribute("data-val", input.getAttribute("data-val"));
            if (input.getAttribute("data-val-required")) select.setAttribute("data-val-required", input.getAttribute("data-val-required"));

            input.parentNode.insertBefore(select, input);
            input.parentNode.removeChild(input);

            initRemoteSelect($(select), mapping[name]);
        });
    }

    window.TrivaUi = {
        initDynamicElements: function (container) {
            initForeignKeySelects(container || document);
        }
    };

    document.addEventListener("DOMContentLoaded", function () {
        initTopbarSearch();
        initListSearch();
        initForeignKeySelects(document);
    });
})();
