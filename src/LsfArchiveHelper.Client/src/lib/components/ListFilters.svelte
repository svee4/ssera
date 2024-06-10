<script lang="ts">
	import { EventsApiHelper } from "../EventsApiHelper";
	import { queryParam } from "sveltekit-search-params";
	import type { EventHandler } from "svelte/elements";
	import type { Writable } from "svelte/store";

	// this component is tightly coupled to the list page

	export let orderBy: EventsApiHelper.OrderByType,
		sort: EventsApiHelper.SortType,
		selectedEventTypes: string[],
		search: string,
		pageSize: number,
		onSubmit: EventHandler<SubmitEvent, HTMLFormElement> | undefined;

	const queryParamOptions = {
		showDefaults: false,
		pushHistory: false,
	};

	const orderByStore = queryParam<EventsApiHelper.OrderByType>(
		"orderBy",
		{
			encode: (value) => value.toString(),
			decode: (value) =>
				value === null
					? "Date"
					: value in EventsApiHelper.AllOrderByTypes
						? (value as EventsApiHelper.OrderByType)
						: "Date",
			defaultValue: "Date",
		},
		queryParamOptions,
	) as Writable<EventsApiHelper.OrderByType>;

	const sortStore = queryParam<EventsApiHelper.SortType>(
		"sort",
		{
			encode: (value) => value.toString(),
			decode: (value) =>
				value === null
					? "Descending"
					: value in EventsApiHelper.AllSortTypes
						? (value as EventsApiHelper.SortType)
						: "Descending",
			defaultValue: "Descending",
		},
		queryParamOptions,
	)  as Writable<EventsApiHelper.SortType>;

	const possibleEventTypes = Object.keys(EventsApiHelper.AllEventTypes);

	const selectedEventTypesStore = queryParam<Record<string, boolean>>(
		"eventTypes",
		{
			encode: (value) => parseEnabledEventTypes(value).join(",") || undefined,
			decode: (value) => {
				const base = possibleEventTypes.reduce(
					(prev, cur) => {
						prev[cur] = false;
						return prev;
					},
					{} as Record<string, boolean>,
				);

				if (value === null) return base;

				value
					.split(",")
					.filter((key) => possibleEventTypes.includes(key))
					.forEach((key) => (base[key] = true));

				return base;
			},
			defaultValue: possibleEventTypes.reduce(
				(prev, cur) => {
					prev[cur] = false;
					return prev;
				},
				{} as Record<string, boolean>,
			),
		},
		queryParamOptions,
	) as Writable<Record<string, boolean>>;

	const searchStore = queryParam("search", {
			encode: value => value,
			decode: value => value ?? "",
			defaultValue: ""
		}, 
		queryParamOptions
	)  as Writable<string>;

	const pageSizeStore = queryParam("pageSize", {
		encode: (v) => v.toString(),
		decode: (v) => (v ? parseInt(v) : 1),
		defaultValue: 20,
	}) as Writable<number>;

	$: orderBy = $orderByStore!;
	$: sort = $sortStore!;
	$: selectedEventTypes = parseEnabledEventTypes($selectedEventTypesStore!);
	$: search = $searchStore!;
	$: pageSize = $pageSizeStore!;

	// tracks if filters are not applied to the displayed list
	let prevQueryParams = getNewPrevQueryParams();
	let dirty = false;

	const submitCallback: EventHandler<SubmitEvent, HTMLFormElement> = (ev) => {
		prevQueryParams = getNewPrevQueryParams();
		dirty = false;
		if (onSubmit) onSubmit(ev);
	};

	$: dirty =
		$sortStore !== prevQueryParams.sort ||
		$orderByStore !== prevQueryParams.orderBy ||
		// again lazy array equality check order is guaranteed
		parseEnabledEventTypes($selectedEventTypesStore!).toString() !==
			prevQueryParams.selectedEventTypes.toString() ||
		$searchStore !== prevQueryParams.search ||
		$pageSizeStore !== prevQueryParams.pageSize;

	function getNewPrevQueryParams() {
		return {
			sort: $sortStore,
			orderBy: $orderByStore,
			selectedEventTypes: parseEnabledEventTypes($selectedEventTypesStore!),
			search: $searchStore,
			pageSize: $pageSizeStore,
		};
	}

	function parseEnabledEventTypes(selectedEventTypes: Record<string, boolean>) {
		return Object.entries(selectedEventTypes)
			.filter(([_, b]) => b)
			.reduce((prev, cur) => [...prev, cur[0]], [] as string[]);
	}

	const dirtyBackgroundColor =  "rgb(255, 245, 254)";
	const getBgColor = <TValue,>(storeValue: TValue, prevValue: TValue) =>
		storeValue !== prevValue ? dirtyBackgroundColor : "";
</script>

<form id="form" on:submit|preventDefault={submitCallback}>
	<h3>Filter</h3>
	<div id="filters">
		<div class="double-container">
			<div class="filter-item">
				<label for="orderby">Order by</label>
				<select
					id="orderby"
					name="orderby"
					bind:value={$orderByStore}
					style:background-color={getBgColor($orderByStore, prevQueryParams.orderBy)}
				>
					{#each Object.entries(EventsApiHelper.AllOrderByTypes) as [key, name]}
						<option value={key}>{name}</option>
					{/each}
				</select>
			</div>
			<div class="filter-item">
				<label for="sort">Sort</label>
				<select
					id="sort"
					name="sort"
					bind:value={$sortStore}
					style:background-color={getBgColor($sortStore, prevQueryParams.sort)}
				>
					{#each Object.entries(EventsApiHelper.AllSortTypes) as [key, name]}
						<option value={key}>{name}</option>
					{/each}
				</select>
			</div>
		</div>

		<div class="filter-item">
			<fieldset>
				<legend>Event type</legend>
				<div id="fieldset">
					{#each Object.entries(EventsApiHelper.AllEventTypes) as [key, name]}
						<div
							style:background-color={(
								$selectedEventTypesStore[key]
									? !prevQueryParams.selectedEventTypes.includes(key)
									: prevQueryParams.selectedEventTypes.includes(key)
							)
								? dirtyBackgroundColor
								: ""}
						>
							<input
								style="padding: 2px"
								style:accent-color={EventsApiHelper.TypeColors[key]}
								id="eventtype-{key}"
								type="checkbox"
								bind:checked={$selectedEventTypesStore[key]}
							/>
							<label style="padding: 2px;" for="eventtype-{key}">{name}</label>
						</div>
					{/each}
				</div>
			</fieldset>
		</div>

		<div class="double-container">
			<div class="filter-item">
				<label for="search">Search title</label>
				<input
					type="search"
					id="search"
					name="search"
					bind:value={$searchStore}
					style:background-color={getBgColor($searchStore, prevQueryParams.search)}
				/>
			</div>
			<div class="filter-item">
				<label for="pageSize">Rows per page</label>
				<input
					type="number"
					id="pageSize"
					name="pageSize"
					bind:value={$pageSizeStore}
					style:background-color={getBgColor($pageSizeStore, prevQueryParams.pageSize)}
					min="10"
					max="5000"
				/>
			</div>
		</div>

		<div class="double-container">
			<div>
				<button id="apply" style="width: 100%;">Apply</button>
			</div>
			<div>
				<p style:visibility={dirty ? "visible" : "hidden"}>Unsaved changes</p>
			</div>
		</div>
	</div>
</form>

<style>
	#filters-container {
		display: flex;
		flex-direction: column;
		gap: 0.75em;
	}

	#filters {
		display: flex;
		gap: 1em;
		align-items: center;
		flex-wrap: wrap;
	}

	.filter-item {
		display: flex;
		flex-direction: column;
	}

	.double-container {
		display: flex;
		flex-direction: column;
		gap: 0.5em;
	}

	#fieldset {
		padding: 4px;
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		width: min-content;
		white-space: nowrap;
	}

	#fieldset > div {
		display: flex;
		margin: 1px;

		& label {
			width: 100%;
		}
	}

	@media screen and (width <= 800px) {
		.double-container {
			flex-direction: row;
			gap: 1em;
		}

		#fieldset {
			grid-template-columns: repeat(2, 1fr);
		}
	}
</style>
