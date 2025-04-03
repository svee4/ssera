<script lang="ts">
	import { onMount } from "svelte";
	import { EventsApiHelper } from "$lib/EventsApiHelper";
	import ListFilters from "$lib/components/ListFilters.svelte";
	import { queryParam } from "sveltekit-search-params";
	import { get } from "svelte/store";
	import PagingDisplay from "$lib/components/PagingDisplay.svelte";

	let response: Promise<EventsApiHelper.ApiResponse> = new Promise(() => {});
	let responsePending = false;

	let orderBy: EventsApiHelper.OrderByType,
		sort: EventsApiHelper.SortType,
		selectedEventTypes: string[],
		search: string,
		pageSize: number | null;

	let pageNumberStore = queryParam("page", {
		encode: (v) => v.toString(),
		decode: (v) => (v ? parseInt(v) : 1),
		defaultValue: 1,
	});

	let pageNumber = get(pageNumberStore)!;
	$: $pageNumberStore = pageNumber;

	let activeFilters: {
		orderBy: EventsApiHelper.OrderByType;
		sort: EventsApiHelper.SortType;
		selectedEventTypes: string[];
		search: string;
		pageSize: number;
		pageNumber: number;
	} = {
		// @ts-ignore
		orderBy,
		// @ts-ignore
		sort,
		// @ts-ignore
		selectedEventTypes,
		// @ts-ignore
		search,
		// @ts-ignore
		pageSize,
		pageNumber,
	};

	function fetchData() {

		if (!pageSize) return;

		if (responsePending) return;
		responsePending = true;

		const params = new URLSearchParams();

		params.append("orderBy", orderBy);
		params.append("sort", sort);
		selectedEventTypes.forEach((key) => params.append("eventTypes", key));

		if (search) {
			params.append("search", search!);
		}

		// quick hack: reset page when search params have changed
		if (orderBy !== activeFilters.orderBy
			|| sort !== activeFilters.sort
			// lazy array equality check, order is guaranteed
			|| selectedEventTypes.toString() !== activeFilters.selectedEventTypes.toString()
			|| search !== activeFilters.search
		) {
			pageNumber = 1;
		}

		params.append("pageSize", pageSize!.toString());
		params.append("page", pageNumber!.toString());

		const oldActiveFilters = { ...activeFilters };
		setActiveFilters();

		response = new Promise(async (resolve, reject) => {
			const resp = await fetch(EventsApiHelper.ApiRoute + "?" + params.toString());
			const json = await resp.json();
			responsePending = false;

			if (resp.ok) resolve(json);
			else {
				activeFilters = oldActiveFilters;
				reject(json);
			}
		});
	}

	function setPageNumber(number: number) {
		if (!number || number < 0) return;
		pageNumber = number;
		fetchData();
	}

	function getPageNumberSetter(totalResults: number) {
		// uhhh currently the result of the fetch is not saved anywhere so we use this
		return (number: number) => {
			if (number > Math.ceil(totalResults / activeFilters.pageSize!)) return;
			setPageNumber(number);
		}
	}

	function setActiveFilters() {
		if (!pageSize) throw new Error("pageSize should not be null when calling setActiveFilters");

		activeFilters = {
			orderBy,
			sort,
			selectedEventTypes,
			search,
			pageSize,
			pageNumber,
		};
	}

	onMount(() => fetchData());
</script>

<svelte:head>
	<title>Events</title>
</svelte:head>

<ListFilters
	bind:orderBy
	bind:sort
	bind:selectedEventTypes
	bind:search
	bind:pageSize
	onSubmit={fetchData}
></ListFilters>

<section id="results-container">
	<h3>Results</h3>
	{#await response}
		<p>Loading...</p>
	{:then data}
		<div id="data-container">
			<PagingDisplay
				pageNumber={activeFilters.pageNumber}
				bind:dirtyPageNumber={pageNumber}
				pageSize={activeFilters.pageSize}
				totalResults={data.totalResults}
				setPageNumber={getPageNumberSetter(data.totalResults)}
			></PagingDisplay>
			<table style="color: black; width: 100%;">
				<thead>
					<tr>
						<th scope="col">#</th>
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
					{#each data.results as event, i}
						<tr
							id={"row-" + i}
							style="background: color-mix(in srgb, {EventsApiHelper.TypeColors[
								event.type
							]}, white 80%)"
						>
							<td class="col-number">
								<span>
									{i + (activeFilters.pageNumber - 1) * activeFilters.pageSize + 1}
								</span>
							</td>
							<td class="col-date">
								<span>{new Date(event.date).toLocaleDateString()}</span>
							</td>
							<td class="col-type">
								<span>
									{EventsApiHelper.AllEventTypes[event.type]}
								</span>
							</td>
							<td class="col-title">
								<span>{event.title ?? ""}</span>
							</td>
							<td class="col-link">
								{#if event.link === null}
									<span>-</span>
								{:else}
									<a href={event.link}>Link</a>
								{/if}
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
			<PagingDisplay
				pageNumber={activeFilters.pageNumber}
				bind:dirtyPageNumber={pageNumber}
				pageSize={activeFilters.pageSize}
				totalResults={data.totalResults}
				setPageNumber={getPageNumberSetter(data.totalResults)}
			></PagingDisplay>
		</div>
	{:catch error}
		<p>Error loading results: {error.message ?? error.title}</p>
	{/await}
</section>

<style>
	#data-container {
		display: flex;
		flex-direction: column;
		gap: 0.5em;
	}

	#results-container {
		display: flex;
		flex-direction: column;
		gap: 0.5em;

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
			}
		}
	}

	.col-date {
		word-break: keep-all;
	}

	*:is(.col-number, .col-date, .col-link) {
		width: min-content;
	}

	.col-type {
		white-space: nowrap;
	}

	@media screen and (width <= 800px) {
		.col-type {
			white-space: normal;
		}
	}
</style>
