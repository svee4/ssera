<script lang="ts">
	
</script>

<div id="container">
	<nav>
		<a href="/">Home</a>
		<a href="/events">Events</a>
		<a href="/gallery">Gallery</a>
		<a href="/history">History</a>
	</nav>
	<main>
		<slot></slot>
	</main>
</div>

<style>
	#container {
		padding: 12px;
		display: flex;
		flex-direction: column;
		gap: 1em;
	}
	nav {
		display: flex;
		gap: 1em;
		align-items: center;
		
		& a {
			font-size: 1.1em;
		}
	}
	main {
		display: flex;
		flex-direction: column;
		gap: 1em;
	}
</style>
