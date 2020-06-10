package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.content;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.service.PreferenceService;

@SpringBootTest
public class AvailabilityControllerTests {

	private MockMvc mockMvc;

	@Mock
	private PreferenceService preferenceService;

	@InjectMocks
	private AvailabilityController availabilityController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(availabilityController).build();
	}

	@Test
	public void getStatus_successful() throws Exception {
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/health/status"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("OK"));
	}
	
	@Test
	public void getMaintenanceMode_true() throws Exception {
		when(preferenceService.isServiceUnavailable()).thenReturn(true);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/maintenance"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("true"));
	}
	
	@Test
	public void getMaintenanceMode_false() throws Exception {
		when(preferenceService.isServiceUnavailable()).thenReturn(false);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/maintenance"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("false"));
	}
	
	@Test
	public void enterMaintenanceMode_successful() throws Exception {
		doNothing().when(preferenceService).setServiceAvailability(true);
		doNothing().when(preferenceService).setServiceAvailability(false);
		mockMvc.perform(MockMvcRequestBuilders.post("/v1/maintenance"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("OK"));
		verify(preferenceService, times(0)).setServiceAvailability(true);
		verify(preferenceService, times(1)).setServiceAvailability(false);
	}
	
	@Test
	public void leaveMaintenanceMode_successful() throws Exception {
		doNothing().when(preferenceService).setServiceAvailability(true);
		doNothing().when(preferenceService).setServiceAvailability(false);
		mockMvc.perform(MockMvcRequestBuilders.delete("/v1/maintenance"))
	    	.andExpect(status().isOk())
	    	.andExpect(content().string("OK"));
		verify(preferenceService, times(1)).setServiceAvailability(true);
		verify(preferenceService, times(0)).setServiceAvailability(false);
	}

}
