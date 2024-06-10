<script lang="ts">
	import { EventsApiHelper } from "../EventsApiHelper";
	import { queryParam } from "sveltekit-search-params";
	import { derived, get, type Writable } from "svelte/store";
	import type { EventHandler } from "svelte/elements";

	export let orderBy: EventsApiHelper.OrderByType,
		sort: EventsApiHelper.SortType,
		selectedEventTypes: string[],
		search: string,
		onSubmit: EventHandler<SubmitEvent, HTMLFormElement> | undefined;

	const queryParamOptions = {
		showDefaults: false,
		pushHistory: false,
	};

	const possibleEventTypes = Object.keys(EventsApiHelper.AllEventTypes);

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
	);

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
	);

	const selectedEventTypesStore = queryParam<Record<string, boolean>>(
		"eventTypes",
		{
			encode: (value) =>
				Object.entries(value)
					.filter(([_, b]) => b)
					.map(([k]) => k)
					.join(",") || undefined,
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
	);

	const searchStore = queryParam("search", undefined, queryParamOptions);

	$: orderBy = $orderByStore!;
	$: sort = $sortStore!;
	$: selectedEventTypes = Object.entries($selectedEventTypesStore!)
		.filter(([_, b]) => b)
		.reduce((prev, cur) => [...prev, cur[0]], [] as string[]);
	$: search = $searchStore!;

	// tracks if filters are not applied to the displayed list
	let filtersAreDirty = false;
	let prevQueryParams = getNewPrevQueryParams();

	const submitCallback: EventHandler<SubmitEvent, HTMLFormElement> = (ev) => {
		prevQueryParams = getNewPrevQueryParams();
		if (onSubmit) onSubmit(ev);
		filtersAreDirty = false;
	};

	$: filtersAreDirty =
		$sortStore !== prevQueryParams.sort ||
		$orderByStore !== prevQueryParams.orderBy ||
		stringifyAllSelectedEventTypes($selectedEventTypesStore!) !==
			prevQueryParams.selectedEventTypes ||
		$searchStore !== prevQueryParams.search;

	function getNewPrevQueryParams() {
		return {
			sort: $sortStore,
			orderBy: $orderByStore,
			selectedEventTypes: stringifyAllSelectedEventTypes($selectedEventTypesStore!),
			search: $searchStore,
		};
	}

	function stringifyAllSelectedEventTypes(selectedEventTypes: Record<string, boolean>) {
		return Object.entries(selectedEventTypes)
			.toSorted((a, b) => a[0].localeCompare(b[0])) // sort by key to keep order
			.map((obj) => (obj[1] ? 1 : 0))
			.join("");
	}
</script>

<form id="form" on:submit|preventDefault={onSubmit}>
	<h3>Filter</h3>
	<div id="filters">
		<div id="order-sort-container">
			<div class="filter-item">
				<label for="orderby">Order by</label>
				<select id="orderby" name="orderby" bind:value={$orderByStore}>
					{#each Object.entries(EventsApiHelper.AllOrderByTypes) as [key, name]}
						<option value={key}>{name}</option>
					{/each}
				</select>
			</div>
			<div class="filter-item">
				<label for="sort">Sort</label>
				<select id="sort" name="sort" bind:value={$sortStore}>
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
						<input
							style="margin: 2px;"
							style:accent-color={EventsApiHelper.TypeColors[key]}
							id="eventtype-{key}"
							type="checkbox"
							bind:checked={$selectedEventTypesStore[key]}
						/>
						<label style="margin: 2px;" for="eventtype-{key}">{name}</label>
					{/each}
				</div>
			</fieldset>
		</div>
		<div class="filter-item">
			<label for="search">Search title</label>
			<input type="search" id="search" name="search" bind:value={$searchStore} />
		</div>

		<div class="filter-item">
			<div>&nbsp;</div>
			<button
				id="apply"
				style="color: black"
				style:border-color={filtersAreDirty ? "blue" : ""}>Apply</button
			>
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

		& input[type="search"] {
			font-size: 1em;
			padding: 4px;
			border: 1px solid black;
			border-radius: 2px;
		}
	}

	.filter-item {
		display: flex;
		flex-direction: column;
	}

	#order-sort-container {
		display: flex;
		flex-direction: column;
		gap: 0.5em;
	}

	#fieldset {
		padding: 4px;
		display: grid;
		grid-template-columns: repeat(6, 1fr);
		width: min-content;
		white-space: nowrap;
	}
</style>
