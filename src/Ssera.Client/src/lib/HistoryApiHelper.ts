import { ApiHelper } from "$lib/ApiHelper";

export namespace HistoryApiHelper {

	export const ApiRoute = `${ApiHelper.ApiDomain}/api/history` as const;

	export type ApiResponse = {
		timestamp: string;
		workerName: string;
		message: string;
	}[];
}
