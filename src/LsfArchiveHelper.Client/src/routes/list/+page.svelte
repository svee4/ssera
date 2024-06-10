<script lang="ts">
	import { onMount } from "svelte";
	import { EventsApiHelper } from "$lib/EventsApiHelper";
	import ListFilters from "$lib/components/ListFilters.svelte";
	import Test from "$lib/components/Test.svelte";

	let response: Promise<EventsApiHelper.ApiResponse> = new Promise(() => {
	});

	let 
        orderBy: EventsApiHelper.OrderByType,
        sort: EventsApiHelper.SortType,
        selectedEventTypes: string[],
        search: string
    ;

	let pageSize,
		pageNumber;

	function fetchData() {
		
		console.log ({
			orderBy,
			sort,
			selectedEventTypes,
			search
		});

		const params = new URLSearchParams();

		params.append("orderBy", orderBy!);
		params.append("sort", sort!);
		selectedEventTypes.forEach(key => params.append("eventTypes", key));

		if (search) {
			params.append("search", search!);
		}

		response = fetch(EventsApiHelper.ApiRoute + "?" + params.toString()).then(response => response.json());
	}


	onMount(() => fetchData());
</script>

<svelte:head>
	<title>List</title>
</svelte:head>

<ListFilters
	bind:orderBy={orderBy}
	bind:sort={sort}
	bind:selectedEventTypes={selectedEventTypes}
	bind:search={search}
	onSubmit={fetchData}
></ListFilters> 

<section id="results-container">
	<h3>Results</h3>
	{#await response}
		<p>Loading...</p>
	{:then data}
		{#if data.lastUpdate}
			<p>Last import: {new Date(data.lastUpdate).toLocaleString()}</p>
		{:else}
			<p>Last update: {"<"}no data{">"}</p>
		{/if}
		<table style="color: black">
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
			{#each data.events as event, i}
				<tr id={"row-" + i} style="background: color-mix(in srgb, {EventsApiHelper.TypeColors[event.type]}, white 80%)">
					<td class="col-number">{i}</td>
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
</section>


<style>

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

			&.col-date {
				word-break:keep-all;
			}

			&:is(.col-number, .col-date, .col-link) {
				width: min-content;
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
