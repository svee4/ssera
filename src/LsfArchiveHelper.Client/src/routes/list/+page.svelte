<script lang="ts">
	import { onMount } from "svelte";
	import { EventsApiHelper } from "$lib/EventsApiHelper";
	import { queryParam, ssp } from "sveltekit-search-params";

	export const ssr = false;

	let stream: Promise<EventsApiHelper.ApiResponse> = new Promise(() => {
	});

	const queryParamOptions = {
		showDefaults: false,
		pushHistory: false,
	};

	const orderBy = queryParam<EventsApiHelper.OrderByType>("orderBy", {
		encode: value => value.toString(),
		decode: value =>
			value === null
				? "Date"
				: value in EventsApiHelper.AllOrderByTypes
					? value as EventsApiHelper.OrderByType
					: "Date",
		defaultValue: "Date",
	}, queryParamOptions);

	const sort = queryParam<EventsApiHelper.SortType>("sort", {
		encode: value => value.toString(),
		decode: value =>
			value === null
				? "Descending"
				: value in EventsApiHelper.AllSortTypes
					? value as EventsApiHelper.SortType
					: "Descending",
		defaultValue: "Descending",
	}, queryParamOptions);

	const possibleEventTypes = Object.keys(EventsApiHelper.AllEventTypes);

	const selectedEventTypes = queryParam<Record<string, boolean>>("eventTypes", {
		encode: value => Object.entries(value).filter(([_, b]) => b).map(([k]) => k).join(","),
		decode: value => {
			const base = possibleEventTypes.reduce((prev, cur) => {
				prev[cur] = false;
				return prev;
			}, {} as Record<string, boolean>);
			
			if (value === null) return base;

			value.split(",")
				.filter(key => possibleEventTypes.includes(key))
				.forEach(key => base[key] = true);
			
			return base;
		},
		defaultValue: possibleEventTypes.reduce((prev, cur) => {
			prev[cur] = false;
			return prev;
		}, {} as Record<string, boolean>),
	}, queryParamOptions);

	const search = queryParam("search", undefined, queryParamOptions);

	function fetchData() {
		const params = new URLSearchParams();
		params.append("orderBy", $orderBy!);
		params.append("sort", $sort!);

		Object.entries($selectedEventTypes!)
			.filter(([_, selected]) => selected)
			.forEach(([key]) => params.append("eventTypes", key));

		if ($search) {
			params.append("search", $search!);
		}

		stream = fetch(EventsApiHelper.ApiRoute + "?" + params.toString()).then(response => response.json());
	}

	onMount(() => fetchData());
</script>

<form id="filters-container" on:submit|preventDefault={fetchData}>
	<h3>Filter</h3>
	<div id="filters">
		<div id="order-sort-group">
			<div class="filter-item">
				<label for="orderby">Order by</label>
				<select id="orderby" name="orderby" bind:value={$orderBy}>
					{#each Object.entries(EventsApiHelper.AllOrderByTypes) as [key, name]}
						<option value={key}>{name}</option>
					{/each}
				</select>
			</div>
			<div class="filter-item">
				<label for="sort">Sort</label>
				<select id="sort" name="sort" bind:value={$sort}>
					{#each Object.entries(EventsApiHelper.AllSortTypes) as [key, name]}
						<option value={key}>{name}</option>
					{/each}
				</select>
			</div>
		</div>

		<div class="filter-item">
			<fieldset id="filter-fieldset">
				<legend>Event type</legend>
				{#each Object.entries(EventsApiHelper.AllEventTypes) as [key, name] }
					<input style="margin: 2px;" id="eventtype-{key}" type="checkbox"
						   bind:checked={$selectedEventTypes[key]} />
					<label style="margin: 2px;" for="eventtype-{key}">{name}</label>
				{/each}
			</fieldset>
		</div>
		<div class="filter-item">
			<label for="search">Search title</label>
			<input type="search" id="search" name="search" bind:value={$search} />
		</div>
		<div class="filter-item">
			<div>&nbsp;</div>
			<button id="apply">Apply</button>
		</div>
	</div>
</form>

<article id="results-container">
	<h3>Results</h3>
	{#await stream}
		<p>Loading...</p>
	{:then events}
		<table>
			<thead>
			<tr>
				<th scope="col">
					<span>Date</span>
				</th>
				<th scope="col">
					<span>Type</span>
				</th>
				<th scope="col">
					<span>Title</span>
				</th>
				<th scope="col">
					<span>Link</span>
				</th>
			</tr>
			</thead>
			<tbody>
			{#each events as event}
				<tr>
					<td class="col-date">
						<span>{event.dateUtc}</span>
					</td>
					<td class="col-type">
						<span>{EventsApiHelper.AllEventTypes[event.type]}</span>
					</td>
					<td class="col-title">
						<span>{event.title ?? ""}</span>
					</td>
					<td class="col-link">
						{#if event.link === null}
							<a href="#">No link</a>
						{:else}
							<a href={event.link}>Link</a>
						{/if}
					</td>
				</tr>
			{/each}
			</tbody>
		</table>
	{:catch error}
		<p>Error loading results: {error.message}</p>
	{/await}
</article>


<style>
	#filters-container, #results-container {
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
			padding: 4px;
		}
	}

	.filter-item {
		display: flex;
		flex-direction: column;
	}

	#order-sort-group {
		display: flex;
		flex-direction: column;
		gap: 0.5em;
	}

	#filter-fieldset {
		padding: 4px;
		display: grid;
		grid-template-columns: repeat(6, 1fr);
		width: min-content;
		white-space: nowrap;
	}

	#results-container {
		& table {
			padding: 6px;
			border: 1px solid black;
			border-collapse: collapse;
		}

		& tr {
			border-top: 1px solid gray;
			background-color: rgb(250, 250, 250);

			&:nth-child(2n) {
				background-color: rgb(242, 242, 242);
			}
		}

		& :is(th, td) {
			padding: 4px;

			&:not(:first-child) {
				border-left: 1px solid gray;
			}

			& > :is(a, span) {
				display: flex;
				justify-content: flex-start;
				align-items: center;

				max-width: 80ch;
			}

			&.col-link > :is(a, span) {
				word-break: break-all;
			}
		}
	}

	@media screen and (width <= 800px) {

		#order-sort-group {
			flex-direction: row;
			gap: 1em;
		}

		#filter-fieldset {
			grid-template-columns: repeat(4, 1fr);
		}

	}

</style>
