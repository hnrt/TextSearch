package com.hideakin.textsearch.index.controller;

import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.service.PreferenceService;

@SpringBootTest
public class PreferenceControllerTests {

	private MockMvc mockMvc;

	@Mock
	private PreferenceService preferenceService;

	@InjectMocks
	private PreferenceController preferenceController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(preferenceController).build();
	}

	@Test
	public void getPreference_successful() throws Exception {
		ValueResponse rsp = new ValueResponse();
		rsp.setValue("bar");
		when(preferenceService.getPreference("foo")).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/preferences/foo"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$.value").value(rsp.getValue()));
	}

	@Test
	public void updatePreference_successful() throws Exception {
		UpdatePreferenceRequest req = new UpdatePreferenceRequest();
		req.setName("quux");
		req.setValue("fred");
		doNothing().when(preferenceService).updatePreference(argThat(x -> x.getName().equals("quux") && x.getValue().equals("fred")));
		ObjectMapper om = new ObjectMapper();
		String json = om.writeValueAsString(req);
		mockMvc.perform(MockMvcRequestBuilders
				.post("/v1/preferences")
				.contentType(MediaType.APPLICATION_JSON)
				.content(json))
			.andExpect(status().isOk());
		verify(preferenceService, times(1)).updatePreference(argThat(x -> x.getName().equals("quux") && x.getValue().equals("fred")));
	}
	
	@Test
	public void deletePreference_successful() throws Exception {
		doNothing().when(preferenceService).deletePreference("thud");
		mockMvc.perform(MockMvcRequestBuilders.delete("/v1/preferences/thud"))
			.andExpect(status().isOk());
		verify(preferenceService, times(1)).deletePreference("thud");
	}

}
